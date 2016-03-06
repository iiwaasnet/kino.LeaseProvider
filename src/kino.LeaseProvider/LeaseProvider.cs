using System;
using System.Collections.Concurrent;
using kino.Consensus;
using kino.Consensus.Configuration;
using kino.Core.Diagnostics;
using kino.Core.Framework;
using LeaseConfiguration = kino.LeaseProvider.Configuration.LeaseConfiguration;

namespace kino.LeaseProvider
{
    public class LeaseProvider : ILeaseProvider
    {
        private readonly IIntercomMessageHub intercomMessageHub;
        private readonly IBallotGenerator ballotGenerator;
        private readonly ISynodConfiguration synodConfig;
        private readonly LeaseConfiguration leaseConfig;
        private readonly ILogger logger;
        private readonly ConcurrentDictionary<Instance, InstanceLeaseProviderHolder> leaseProviders;
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
            leaseProviders = new ConcurrentDictionary<Instance, InstanceLeaseProviderHolder>();
        }

        public void Start()
        {
            intercomMessageHub.Start();
            RegisterInstanceLeaseProvider(new Instance("A"));
            RegisterInstanceLeaseProvider(new Instance("B"));
        }

        public void Stop()
        {
            intercomMessageHub.Stop();
        }

        public Lease GetLease(Instance instance, TimeSpan leaseTimeSpan, byte[] requestorIdentity)
        {
            ValidateLeaseTimeSpan(leaseTimeSpan);

            InstanceLeaseProviderHolder leaseProviderHolder;
            if (!leaseProviders.TryGetValue(instance, out leaseProviderHolder))
            {
                throw new Exception($"LeaseProvider for Instance {instance.Identity.GetString()} is not registered!");
            }
            if (leaseProviderHolder.InstanceLeaseProvider == null)
            {
                throw new Exception($"LeaseProvider for Instance {instance.Identity.GetString()} will be available " +
                                    $"in at most {leaseConfig.MaxAllowedLeaseTimeSpan.TotalMilliseconds} ms.");
            }

            return leaseProviderHolder.InstanceLeaseProvider.GetLease(requestorIdentity, leaseTimeSpan);
        }

        public RegistrationResult RegisterInstanceLeaseProvider(Instance instance)
        {
            var leaseProvider = leaseProviders.GetOrAdd(instance, CreateInstanceLeaseProviderHolder);

            return new RegistrationResult
                   {
                       ActivationWaitTime = leaseProvider.InstanceLeaseProvider != null
                                                ? TimeSpan.Zero
                                                : leaseConfig.MaxAllowedLeaseTimeSpan
                   };
        }

        private InstanceLeaseProviderHolder CreateInstanceLeaseProviderHolder(Instance instance)
            => new InstanceLeaseProviderHolder(instance,
                                               intercomMessageHub,
                                               ballotGenerator,
                                               synodConfig,
                                               new Consensus.Configuration.LeaseConfiguration
                                               {
                                                   ClockDrift = leaseConfig.ClockDrift,
                                                   MaxLeaseTimeSpan = leaseConfig.MinAllowedLeaseTimeSpan,
                                                   NodeResponseTimeout = leaseConfig.NodeResponseTimeout,
                                                   MessageRoundtrip = leaseConfig.MessageRoundtrip
                                               },
                                               logger);

        private void ValidateLeaseTimeSpan(TimeSpan leaseTimeSpan)
        {
            if (leaseTimeSpan < leaseConfig.MinAllowedLeaseTimeSpan
                || leaseTimeSpan > leaseConfig.MaxAllowedLeaseTimeSpan)
            {
                throw new ArgumentException($"Requested {nameof(leaseTimeSpan)} ({leaseTimeSpan.TotalMilliseconds} ms) " +
                                            $"is not in {leaseConfig.MinAllowedLeaseTimeSpan.TotalMilliseconds}-" +
                                            $"{leaseConfig.MaxAllowedLeaseTimeSpan.TotalMilliseconds} ms range!");
            }
        }

        private void ValidateConfiguration(LeaseConfiguration config)
        {
            if (config.NodeResponseTimeout.TotalMilliseconds * 2 > config.MessageRoundtrip.TotalMilliseconds)
            {
                throw new Exception($"{nameof(config.NodeResponseTimeout)}[{config.NodeResponseTimeout.TotalMilliseconds} msec] " +
                                    "should be at least 2 times shorter than " +
                                    $"{nameof(config.MessageRoundtrip)}[{config.MessageRoundtrip.TotalMilliseconds} msec]");
            }
            if (config.MinAllowedLeaseTimeSpan
                - TimeSpan.FromTicks(config.MessageRoundtrip.Ticks * 2)
                - config.ClockDrift <= TimeSpan.Zero)
            {
                throw new Exception($"{nameof(config.MinAllowedLeaseTimeSpan)}[{config.MinAllowedLeaseTimeSpan.TotalMilliseconds} msec] " +
                                    "should be longer than " +
                                    $"(2 * {nameof(config.MessageRoundtrip)}[{config.MessageRoundtrip.TotalMilliseconds} msec] " +
                                    $"+ {nameof(config.ClockDrift)}[{config.ClockDrift.TotalMilliseconds} msec])");
            }
        }
    }
}