using System;
using System.Collections.Generic;
using kino.Consensus;
using kino.Consensus.Configuration;
using kino.Core.Diagnostics;

namespace kino.LeaseProvider
{
    public class LeaseProvider : ILeaseProvider
    {
        private readonly IIntercomMessageHub intercomMessageHub;
        private readonly IBallotGenerator ballotGenerator;
        private readonly ISynodConfiguration synodConfig;
        private readonly LeaseConfiguration leaseConfig;
        private readonly ILogger logger;
        private readonly IDictionary<Instance, IInstanceLeaseProvider> leaseProviders;
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
            leaseProviders = new Dictionary<Instance, IInstanceLeaseProvider>();

            intercomMessageHub.Start();
        }

        public Lease GetLease(Instance instance, byte[] ownerPayload)
        {
            IInstanceLeaseProvider leaseProvider;

            lock (@lock)
            {
                if (!leaseProviders.TryGetValue(instance, out leaseProvider))
                {
                    leaseProvider = new InstanceLeaseProvider(instance,
                                                              new InstanceRoundBasedRegister(instance,
                                                                                             intercomMessageHub,
                                                                                             ballotGenerator,
                                                                                             synodConfig,
                                                                                             leaseConfig,
                                                                                             logger),
                                                              ballotGenerator,
                                                              leaseConfig,
                                                              synodConfig,
                                                              logger);
                    leaseProviders[instance] = leaseProvider;
                }
            }

            return leaseProvider.GetLease(ownerPayload);
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