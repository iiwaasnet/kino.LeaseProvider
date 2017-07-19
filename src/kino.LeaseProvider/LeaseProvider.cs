using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using kino.Client;
using kino.Consensus;
using kino.Consensus.Configuration;
using kino.Core.Diagnostics;
using kino.Core.Framework;
using kino.LeaseProvider.Configuration;
using kino.LeaseProvider.Messages;
using kino.Messaging;
using Lease = kino.Consensus.Lease;

namespace kino.LeaseProvider
{
    public class LeaseProvider : ILeaseProvider
    {
        private readonly IIntercomMessageHub intercomMessageHub;
        private readonly IBallotGenerator ballotGenerator;
        private readonly ISynodConfiguration synodConfig;
        private readonly InstanceLeaseProviderConfiguration leaseConfiguration;
        private readonly byte[] clusterName;
        private readonly IMessageHub messageHub;
        private readonly ILogger logger;
        private readonly ConcurrentDictionary<Instance, DelayedInstanceWrap> leaseProviders;
        private readonly TimeSpan staleInstancesCleanupPeriod;
        private Timer cleanUpTimer;

        public LeaseProvider(IIntercomMessageHub intercomMessageHub,
                             IBallotGenerator ballotGenerator,
                             ISynodConfiguration synodConfig,
                             InstanceLeaseProviderConfiguration leaseConfiguration,
                             LeaseProviderConfiguration leaseProviderConfiguration,
                             IMessageHub messageHub,
                             ILogger logger)
        {
            ValidateConfiguration(leaseConfiguration);

            this.intercomMessageHub = intercomMessageHub;
            this.ballotGenerator = ballotGenerator;
            this.synodConfig = synodConfig;
            this.leaseConfiguration = leaseConfiguration;
            this.messageHub = messageHub;
            this.logger = logger;
            clusterName = leaseProviderConfiguration.ClusterName.GetBytes();
            staleInstancesCleanupPeriod = leaseProviderConfiguration.StaleInstancesCleanupPeriod;
            leaseProviders = new ConcurrentDictionary<Instance, DelayedInstanceWrap>();
        }

        public bool Start(TimeSpan startTimeout)
        {
            if (intercomMessageHub.Start(startTimeout))
            {
                RequestInstanceDiscovery();
                cleanUpTimer = new Timer(_ => CleanUpStaleInstances(),
                                         null,
                                         TimeSpan.Zero,
                                         staleInstancesCleanupPeriod);
                return true;
            }

            return false;
        }

        private void CleanUpStaleInstances()
        {
            var staleInstances = leaseProviders.Where(kv => kv.Value.InstanceLeaseProvider.IsInstanceStale())
                                               .Select(kv => kv.Key)
                                               .ToList();
            foreach (var staleInstance in staleInstances)
            {
                if (leaseProviders.TryRemove(staleInstance, out var wrap))
                {
                    wrap.InstanceLeaseProvider.Dispose();

                    logger.Warn($"LeaseProvider for Instance [{staleInstance.Identity.GetAnyString()}] "
                                + $"is cleaned up due to inactivity of [{leaseConfiguration.LeaseProviderIsStaleAfter.TotalSeconds}] sec.");
                }
            }
        }

        public void Stop()
        {
            cleanUpTimer?.Dispose();
            intercomMessageHub.Stop();
            leaseProviders.Values.ForEach(v => v.InstanceLeaseProvider.Dispose());
            leaseProviders.Clear();
        }

        public Lease GetLease(Instance instance, GetLeaseRequest leaseRequest)
        {
            ValidateLeaseTimeSpan(leaseRequest.LeaseTimeSpan);

            DelayedInstanceWrap delayedWrap;
            if (!leaseProviders.TryGetValue(instance, out delayedWrap))
            {
                RequestInstanceDiscovery();

                throw new InstanceNotRegisteredException(instance.Identity.GetAnyString());
            }
            if (delayedWrap.InstanceLeaseProvider == null)
            {
                throw new Exception($"LeaseProvider for Instance {instance.Identity.GetAnyString()} will be available " +
                                    $"in at most {leaseConfiguration.ClockDrift.TotalMilliseconds} ms.");
            }

            return delayedWrap.InstanceLeaseProvider.GetLease(new GetLeaseRequest
                                                              {
                                                                  LeaseTimeSpan = leaseRequest.LeaseTimeSpan,
                                                                  RequestorIdentity = leaseRequest.RequestorIdentity,
                                                                  MinValidityTimeFraction = leaseRequest.MinValidityTimeFraction
                                                              });
        }

        private void RequestInstanceDiscovery()
            => messageHub.SendOneWay(Message.Create(new InternalDiscoverLeaseProviderInstancesRequestMessage
                                                    {
                                                        Partition = clusterName
                                                    },
                                                    DistributionPattern.Broadcast));

        public RegistrationResult RegisterInstance(Instance instance)
        {
            var leaseProvider = leaseProviders.GetOrAdd(instance, CreateDelayedInstanceLeaseProvider);

            var res = new RegistrationResult
                      {
                          ActivationWaitTime = leaseProvider.InstanceLeaseProvider != null
                                                   ? TimeSpan.Zero
                                                   : leaseConfiguration.ClockDrift
                      };

            logger.Trace($"Requested LeaseProvider for Instance {instance.Identity.GetAnyString()} " +
                         $"will be active in {res.ActivationWaitTime.TotalSeconds} sec.");

            return res;
        }

        public IEnumerable<Instance> GetRegisteredInstances()
            => leaseProviders.Keys.ToList();

        public InstanceStatus GetInstanceStatus(Instance instance)
        {
            if (leaseProviders.TryGetValue(instance, out var wrap))
            {
                return new InstanceStatus
                       {
                           Instance = instance,
                           ConsensusIsReached = wrap.InstanceLeaseProvider.IsConsensusReached()
                       };
            }

            throw new Exception($"Instance {instance.Identity} is not found!");
        }

        private DelayedInstanceWrap CreateDelayedInstanceLeaseProvider(Instance instance)
            => new DelayedInstanceWrap(instance,
                                       intercomMessageHub,
                                       ballotGenerator,
                                       synodConfig,
                                       leaseConfiguration,
                                       logger);

        private void ValidateLeaseTimeSpan(TimeSpan leaseTimeSpan)
        {
            if (leaseTimeSpan < leaseConfiguration.MaxLeaseTimeSpan)
            {
                throw new ArgumentException($"Requested {nameof(leaseTimeSpan)} ({leaseTimeSpan.TotalMilliseconds} ms) " +
                                            $"should be longer than {leaseConfiguration.MaxLeaseTimeSpan.TotalMilliseconds} ms!");
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
            if (config.MaxLeaseTimeSpan
                - config.MessageRoundtrip.MultiplyBy(2)
                - config.ClockDrift <= TimeSpan.Zero)
            {
                throw new Exception($"{nameof(config.MaxLeaseTimeSpan)}[{config.MaxLeaseTimeSpan.TotalMilliseconds} msec] " +
                                    "should be longer than " +
                                    $"(2 * {nameof(config.MessageRoundtrip)}[{config.MessageRoundtrip.TotalMilliseconds} msec] " +
                                    $"+ {nameof(config.ClockDrift)}[{config.ClockDrift.TotalMilliseconds} msec])");
            }
        }
    }
}