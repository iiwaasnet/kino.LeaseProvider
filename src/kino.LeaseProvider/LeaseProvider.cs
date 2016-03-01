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
        }

        public void Start()
        {
            intercomMessageHub.Start();
            CreateInstanceLeaseProvider(new Instance("A"), TimeSpan.FromSeconds(5));
            CreateInstanceLeaseProvider(new Instance("B"), TimeSpan.FromSeconds(5));
        }

        public void Stop()
        {
            intercomMessageHub.Stop();
        }

        public Lease GetLease(Instance instance, TimeSpan leaseTimeSpan, byte[] requestorIdentity, TimeSpan requestTimeout)
        {
            //throw new NotImplementedException("Timeout");

            ValidateLeaseTimeSpan(leaseTimeSpan);

            var leaseProvider = CreateInstanceLeaseProvider(instance, leaseTimeSpan);

            return leaseProvider.GetLease(requestorIdentity);
        }

        private IInstanceLeaseProvider CreateInstanceLeaseProvider(Instance instance, TimeSpan leaseTimeSpan)
        {
            IInstanceLeaseProvider leaseProvider;

            lock (@lock)
            {
                if (!leaseProviders.TryGetValue(instance, out leaseProvider))
                {
                    var instanceLeaseConfig = Clone(leaseConfig);
                    instanceLeaseConfig.MaxLeaseTimeSpan = leaseTimeSpan;

                    leaseProvider = new InstanceLeaseProvider(instance,
                                                              new InstanceRoundBasedRegister(instance,
                                                                                             intercomMessageHub,
                                                                                             ballotGenerator,
                                                                                             synodConfig,
                                                                                             leaseConfig,
                                                                                             logger),
                                                              ballotGenerator,
                                                              instanceLeaseConfig,
                                                              synodConfig,
                                                              logger);
                    leaseProviders[instance] = leaseProvider;
                }
            }
            return leaseProvider;
        }

        private LeaseConfiguration Clone(LeaseConfiguration src)
            => new LeaseConfiguration
               {
                   ClockDrift = src.ClockDrift,
                   MaxLeaseTimeSpan = src.MaxLeaseTimeSpan,
                   MessageRoundtrip = src.MessageRoundtrip,
                   NodeResponseTimeout = src.NodeResponseTimeout
               };

        private void ValidateLeaseTimeSpan(TimeSpan leaseTimeSpan)
        {
            if (leaseTimeSpan < leaseConfig.MaxLeaseTimeSpan)
            {
                throw new ArgumentException($"Requested LeaseTimeSpan ({leaseTimeSpan.TotalMilliseconds} ms) " +
                                            $"should be longer than min allowed {leaseConfig.MaxLeaseTimeSpan.TotalMilliseconds} ms!");
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