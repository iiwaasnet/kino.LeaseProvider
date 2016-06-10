using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using kino.Client;
using kino.Consensus;
using kino.Consensus.Configuration;
using kino.Core.Diagnostics;
using kino.Core.Framework;
using kino.Core.Messaging;
using kino.LeaseProvider.Configuration;
using kino.LeaseProvider.Messages;
using Lease = kino.Consensus.Lease;

namespace kino.LeaseProvider
{
    public class LeaseProvider : ILeaseProvider
    {
        private readonly IIntercomMessageHub intercomMessageHub;
        private readonly IBallotGenerator ballotGenerator;
        private readonly ISynodConfiguration synodConfig;
        private readonly LeaseTimingConfiguration leaseTimingConfig;
        private readonly LeaseConfiguration leaseConfiguration;
        private readonly IMessageHub messageHub;
        private readonly ILogger logger;
        private readonly ConcurrentDictionary<Instance, DelayedInstanceWrap> leaseProviders;

        public LeaseProvider(IIntercomMessageHub intercomMessageHub,
                             IBallotGenerator ballotGenerator,
                             ISynodConfiguration synodConfig,
                             LeaseTimingConfiguration leaseTimingConfig,
                             LeaseConfiguration leaseConfiguration,
                             IMessageHub messageHub,
                             ILogger logger)
        {
            ValidateConfiguration(leaseTimingConfig);

            this.intercomMessageHub = intercomMessageHub;
            this.ballotGenerator = ballotGenerator;
            this.synodConfig = synodConfig;
            this.leaseTimingConfig = leaseTimingConfig;
            this.leaseConfiguration = leaseConfiguration;
            this.messageHub = messageHub;
            this.logger = logger;
            leaseProviders = new ConcurrentDictionary<Instance, DelayedInstanceWrap>();
        }

        public void Start()
        {
            intercomMessageHub.Start();

            RequestInstanceDiscovery();
        }

        public void Stop()
        {
            intercomMessageHub.Stop();
        }

        public Lease GetLease(Instance instance, TimeSpan leaseTimeSpan, byte[] requestorIdentity)
        {
            ValidateLeaseTimeSpan(leaseTimeSpan);

            DelayedInstanceWrap delayedWrap;
            if (!leaseProviders.TryGetValue(instance, out delayedWrap))
            {
                RequestInstanceDiscovery();

                throw new Exception($"LeaseProvider for Instance {instance.Identity.GetString()} is not registered!");
            }
            if (delayedWrap.InstanceLeaseProvider == null)
            {
                throw new Exception($"LeaseProvider for Instance {instance.Identity.GetString()} will be available " +
                                    $"in at most {leaseTimingConfig.ClockDrift.TotalMilliseconds} ms.");
            }

            return delayedWrap.InstanceLeaseProvider.GetLease(requestorIdentity, leaseTimeSpan);
        }

        private void RequestInstanceDiscovery()
            => messageHub.SendOneWay(Message.Create(new InternalDiscoverLeaseProviderInstancesRequestMessage(),
                                                    DistributionPattern.Broadcast));

        public RegistrationResult RegisterInstance(Instance instance)
        {
            var leaseProvider = leaseProviders.GetOrAdd(instance, CreateDelayedInstanceLeaseProvider);

            var res = new RegistrationResult
                      {
                          ActivationWaitTime = leaseProvider.InstanceLeaseProvider != null
                                                   ? TimeSpan.Zero
                                                   : leaseTimingConfig.ClockDrift
                      };

            logger.Trace($"Requested LeaseProvider for Instance {instance.Identity.GetString()} " +
                         $"will be active in {res.ActivationWaitTime.TotalSeconds} sec.");

            return res;
        }

        public IEnumerable<Instance> GetRegisteredInstances()
            => leaseProviders.Keys.ToList();

        private DelayedInstanceWrap CreateDelayedInstanceLeaseProvider(Instance instance)
            => new DelayedInstanceWrap(instance,
                                       intercomMessageHub,
                                       ballotGenerator,
                                       synodConfig,
                                       leaseConfiguration,
                                       logger);

        private void ValidateLeaseTimeSpan(TimeSpan leaseTimeSpan)
        {
            if (leaseTimeSpan < leaseTimingConfig.MinAllowedLeaseTimeSpan)
            {
                throw new ArgumentException($"Requested {nameof(leaseTimeSpan)} ({leaseTimeSpan.TotalMilliseconds} ms) " +
                                            $"should be longer than {leaseTimingConfig.MinAllowedLeaseTimeSpan.TotalMilliseconds} ms!");
            }
        }

        private void ValidateConfiguration(LeaseTimingConfiguration config)
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