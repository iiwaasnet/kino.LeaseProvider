using System;
using System.Collections.Concurrent;
using kino.Consensus;
using kino.Consensus.Configuration;
using kino.Core.Diagnostics;
using kino.Core.Framework;

namespace kino.LeaseProvider
{
    public class LeaseProvider : ILeaseProvider
    {
        private readonly IIntercomMessageHub intercomMessageHub;
        private readonly IBallotGenerator ballotGenerator;
        private readonly ISynodConfiguration synodConfig;
        private readonly LeaseConfiguration leaseConfig;
        private readonly ILogger logger;
        private readonly ConcurrentDictionary<Instance, InstanceLeaseProviderBag> leaseProviders;
        private readonly object @lock = new object();

        public LeaseProvider(IIntercomMessageHub intercomMessageHub,
                             IBallotGenerator ballotGenerator,
                             ISynodConfiguration synodConfig,
                             LeaseConfiguration leaseConfig,
                             ILogger logger)
        {
            ValidateConfiguration(leaseConfig);

            this.intercomMessageHub = intercomMessageHub;
            this.ballotGenerator = ballotGenerator;
            this.synodConfig = synodConfig;
            this.leaseConfig = leaseConfig;
            this.logger = logger;
            leaseProviders = new ConcurrentDictionary<Instance, InstanceLeaseProviderBag>();
        }

        public void Start()
        {
            intercomMessageHub.Start();
            GetOrCreateInstanceLeaseProvider(new Instance("A", TimeSpan.FromSeconds(5)));
            GetOrCreateInstanceLeaseProvider(new Instance("B", TimeSpan.FromSeconds(5)));
        }

        public void Stop()
        {
            intercomMessageHub.Stop();
        }

        public Lease GetLease(Instance instance, TimeSpan leaseTimeSpan, byte[] requestorIdentity)
        {
            ValidateLeaseTimeSpan(instance, leaseTimeSpan);

            var leaseProvider = GetOrCreateInstanceLeaseProvider(instance);

            return leaseProvider.GetLease(requestorIdentity, leaseTimeSpan);
        }

        public void EnsureInstanceLeaseProviderExists(Instance instance)
        {
            GetOrCreateInstanceLeaseProvider(instance);
        }

        private IInstanceLeaseProvider GetOrCreateInstanceLeaseProvider(Instance instance)
        {
            var providerCreated = false;
            var leaseProviderBag = leaseProviders.GetOrAdd(instance, i => CreateInstanceLeaseProviderBag(i, out providerCreated));

            if (providerCreated)
            {
                instance.MaxAllowedLeaseTimeSpan.Sleep();
            }
            else
            {
                if (instance.MaxAllowedLeaseTimeSpan != leaseProviderBag.MaxAllowedLeaseTimeSpan)
                {
                    throw new ArgumentException($"{nameof(instance.MaxAllowedLeaseTimeSpan)} value {instance.MaxAllowedLeaseTimeSpan.TotalMilliseconds} ms " +
                                                $"is not equal to {leaseProviderBag.MaxAllowedLeaseTimeSpan.TotalMilliseconds} ms " +
                                                $"of existing Instance {instance.Identity.GetString()}");
                }
            }

            return leaseProviderBag.InstanceLeaseProvider;
        }

        private InstanceLeaseProviderBag CreateInstanceLeaseProviderBag(Instance instance, out bool created)
        {
            created = true;
            return new InstanceLeaseProviderBag
                   {
                       InstanceLeaseProvider = new InstanceLeaseProvider(instance,
                                                                         new InstanceRoundBasedRegister(instance,
                                                                                                        intercomMessageHub,
                                                                                                        ballotGenerator,
                                                                                                        synodConfig,
                                                                                                        leaseConfig,
                                                                                                        logger),
                                                                         ballotGenerator,
                                                                         leaseConfig,
                                                                         synodConfig,
                                                                         logger),
                       MaxAllowedLeaseTimeSpan = instance.MaxAllowedLeaseTimeSpan
                   };
        }

        private void ValidateLeaseTimeSpan(Instance instance, TimeSpan leaseTimeSpan)
        {
            if (leaseTimeSpan <= leaseConfig.MaxLeaseTimeSpan)
            {
                throw new ArgumentException($"Requested {nameof(leaseTimeSpan)} ({leaseTimeSpan.TotalMilliseconds} ms) " +
                                            $"should be longer than min allowed {leaseConfig.MaxLeaseTimeSpan.TotalMilliseconds} ms!");
            }
            if (leaseTimeSpan >= instance.MaxAllowedLeaseTimeSpan)
            {
                throw new ArgumentException($"Requested {nameof(leaseTimeSpan)} ({leaseTimeSpan.TotalMilliseconds} ms) " +
                                            $"should be shorter than max allowed {instance.MaxAllowedLeaseTimeSpan.TotalMilliseconds} ms " +
                                            $"for {nameof(instance.Identity)} {instance.Identity.GetString()}!");
            }
        }

        private void ValidateConfiguration(LeaseConfiguration config)
        {
            if (config.NodeResponseTimeout.TotalMilliseconds * 2 > config.MessageRoundtrip.TotalMilliseconds)
            {
                throw new Exception("NodeResponseTimeout[{config.NodeResponseTimeout.TotalMilliseconds} msec] " +
                                    "should be at least 2 times shorter than " +
                                    "MessageRoundtrip[{config.MessageRoundtrip.TotalMilliseconds} msec]");
            }
            if (config.MaxLeaseTimeSpan
                - TimeSpan.FromTicks(config.MessageRoundtrip.Ticks * 2)
                - config.ClockDrift <= TimeSpan.Zero)
            {
                throw new Exception($"MaxLeaseTimeSpan[{config.MaxLeaseTimeSpan.TotalMilliseconds} msec] " +
                                    "should be longer than " +
                                    $"(2 * MessageRoundtrip[{config.MessageRoundtrip.TotalMilliseconds} msec] " +
                                    $"+ ClockDrift[{config.ClockDrift.TotalMilliseconds} msec])");
            }
        }
    }
}