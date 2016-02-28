﻿using System;
using System.Collections.Generic;
using System.Threading;
using Autofac;
using kino.Actors;
using kino.Client;
using kino.Core.Connectivity;
using kino.Core.Diagnostics;
using kino.Core.Messaging;
using kino.Core.Sockets;
using kino.LeaseProvider.Messages;

namespace kino.LeaseProvider.Client
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new MainModule());
            var container = builder.Build();

            var componentResolver = new Composer(container.ResolveOptional<SocketConfiguration>());

            var messageRouter = componentResolver.BuildMessageRouter(container.Resolve<RouterConfiguration>(),
                                                                     container.Resolve<ClusterMembershipConfiguration>(),
                                                                     container.Resolve<IEnumerable<RendezvousEndpoint>>(),
                                                                     container.Resolve<ILogger>());
            messageRouter.Start();
            // Needed to let router bind to socket over INPROC. To be fixed by NetMQ in future.
            Thread.Sleep(TimeSpan.FromMilliseconds(30));

            var messageHub = componentResolver.BuildMessageHub(container.Resolve<MessageHubConfiguration>(),
                                                               container.Resolve<ILogger>());
            messageHub.Start();

            // Host Actors
            var actorHostManager = componentResolver.BuildActorHostManager(container.Resolve<RouterConfiguration>(),
                                                                          container.Resolve<ILogger>());
            foreach (var actor in container.Resolve<IEnumerable<IActor>>())
            {
                actorHostManager.AssignActor(actor);
            }

            Thread.Sleep(TimeSpan.FromSeconds(5));
            Console.WriteLine($"Client is running... {DateTime.Now}");

            while (true)
            {
                var request = Message.CreateFlowStartMessage(new LeaseRequestMessage
                {
                    Instance = "A",
                    LeaseTimeSpan = TimeSpan.FromSeconds(5),
                    RequestTimeout = TimeSpan.FromSeconds(1),
                    Requestor = new Messages.Node
                    {
                        Identity = Guid.NewGuid().ToByteArray(),
                        Uri = "tpc://localhost"
                    }
                });
                request.TraceOptions = MessageTraceOptions.None;
                var callbackPoint = CallbackPoint.Create<LeaseResponseMessage>();
                var promise = messageHub.EnqueueRequest(request, callbackPoint);
                var waitTimeout = TimeSpan.FromSeconds(5);
                if (promise.GetResponse().Wait(waitTimeout))
                {
                    var response = promise.GetResponse().Result.GetPayload<LeaseResponseMessage>();

                    Console.WriteLine($"{DateTime.UtcNow} " +
                                      $"Aquired: {response.LeaseAquired} " +
                                      $"Instance: {response.Lease?.Instance} " +
                                      $"Owner: {response.Lease?.Owner.Uri} " +
                                      $"ExpiresAt: {response.Lease?.ExpiresAt}");
                }
                else
                {
                    Console.WriteLine($"Call timed out after {waitTimeout.TotalSeconds} sec.");
                }
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
            

            Console.ReadLine();
            messageHub.Stop();
            messageRouter.Stop();
            container.Dispose();

            Console.WriteLine("Client stopped.");
        }
    }
}