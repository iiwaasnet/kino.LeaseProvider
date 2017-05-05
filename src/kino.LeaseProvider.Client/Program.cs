using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autofac;
using Autofac.kino;
using kino.Actors;
using kino.Client;
using kino.Core;
using kino.Core.Framework;
using kino.LeaseProvider.Messages;
using kino.Messaging;
using kino.Routing;
using Node = kino.LeaseProvider.Messages.Node;

namespace kino.LeaseProvider.Client
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<MainModule>();
            builder.RegisterModule<KinoModule>();
            var container = builder.Build();

            var messageRouter = container.Resolve<IMessageRouter>();
            messageRouter.Start();
            // Needed to let router bind to socket over INPROC. To be fixed by NetMQ in future.
            Thread.Sleep(TimeSpan.FromMilliseconds(30));

            var messageHub = container.Resolve<IMessageHub>();
            messageHub.Start();

            // Host Actors
            var actorHostManager = container.Resolve<IActorHostManager>();
            foreach (var actor in container.Resolve<IEnumerable<IActor>>())
            {
                actorHostManager.AssignActor(actor);
            }

            Thread.Sleep(TimeSpan.FromSeconds(5));
            Console.WriteLine($"Client is running... {DateTime.Now}");

            var instances = Enumerable.Range(0, 1).Select(i => i.ToString()).ToArray();
            var rnd = new Random(DateTime.UtcNow.Millisecond);

            var partition = "test".GetBytes();

            CreateLeaseProviderInstances(instances, messageHub, partition);

            var run = 0;
            var ownerIdentity = Guid.NewGuid().ToByteArray();
            while (true)
            {
                var leaseTimeSpan = TimeSpan.FromSeconds(5);
                var request = Message.CreateFlowStartMessage(new LeaseRequestMessage
                                                             {
                                                                 Instance = instances[rnd.Next(0, instances.Length - 1)],
                                                                 LeaseTimeSpan = leaseTimeSpan,
                                                                 Requestor = new Node
                                                                             {
                                                                                 Identity = ownerIdentity,
                                                                                 Uri = "tpc://localhost"
                                                                             },
                                                                 Partition = partition,
                                                                 MinValidityTimeFraction = 3
                                                             });
                request.TraceOptions = MessageTraceOptions.None;
                var callbackPoint = new CallbackPoint(MessageIdentifier.Create<LeaseResponseMessage>(partition));
                var promise = messageHub.EnqueueRequest(request, callbackPoint);
                var waitTimeout = TimeSpan.FromMilliseconds(500);
                if (promise.GetResponse().Wait(waitTimeout))
                {
                    var response = promise.GetResponse().Result.GetPayload<LeaseResponseMessage>();

                    if (response.LeaseAcquired)
                    {
                        Console.WriteLine($"{DateTime.UtcNow} " +
                                          $"Acquired: {response.LeaseAcquired} " +
                                          $"Instance: {response.Lease?.Instance} " +
                                          $"Owner: {response.Lease?.Owner.Uri} " +
                                          $"OwnerIdentity: {response.Lease?.Owner.Identity.GetAnyString()} " +
                                          $"RequestorIdentity: {ownerIdentity.GetString()} " +
                                          $"ExpiresAt: {response.Lease?.ExpiresAt}");
                        leaseTimeSpan.DivideBy(2).Sleep();
                    }
                    else
                    {
                        Console.WriteLine("Not a Leader");
                    }
                }
                else
                {
                    Console.WriteLine($"Call timed out after {waitTimeout.TotalSeconds} sec.");
                }
                TimeSpan.FromSeconds(2).Sleep();
            }

            Console.ReadLine();
            messageHub.Stop();
            messageRouter.Stop();
            container.Dispose();

            Console.WriteLine("Client stopped.");
        }

        private static void CreateLeaseProviderInstances(IEnumerable<string> instances, IMessageHub messageHub, byte[] partition)
        {
            if (instances.Any())
            {
                var results = new List<CreateLeaseProviderInstanceResponseMessage>();
                foreach (var instance in instances)
                {
                    var message = Message.CreateFlowStartMessage(new CreateLeaseProviderInstanceRequestMessage
                                                                 {
                                                                     Instance = instance,
                                                                     Partition = partition
                                                                 });
                    //message.TraceOptions = MessageTraceOptions.Routing;
                    var callback = new CallbackPoint(MessageIdentifier.Create<CreateLeaseProviderInstanceResponseMessage>(partition));

                    using (var promise = messageHub.EnqueueRequest(message, callback))
                    {
                        results.Add(promise.GetResponse().Result.GetPayload<CreateLeaseProviderInstanceResponseMessage>());
                    }
                }
                var activationWaitTime = results.Max(r => r.ActivationWaitTime);

                Console.WriteLine($"Waiting {activationWaitTime.TotalSeconds} sec before LeaseProvider Instances are active...");

                if (activationWaitTime > TimeSpan.Zero)
                {
                    Thread.Sleep(activationWaitTime);
                }
            }
        }
    }
}