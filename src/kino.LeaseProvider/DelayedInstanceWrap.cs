using System.Threading;
using System.Threading.Tasks;
using kino.Consensus;
using kino.Consensus.Configuration;
using kino.Core.Diagnostics;
using kino.LeaseProvider.Configuration;

namespace kino.LeaseProvider
{
    internal class DelayedInstanceWrap
    {
        private readonly IIntercomMessageHub intercomMessageHub;
        private readonly IBallotGenerator ballotGenerator;
        private readonly ISynodConfigurationProvider synodConfigProvider;
        private readonly InstanceLeaseProviderConfiguration leaseConfig;
        private readonly ILogger logger;
        private InstanceLeaseProvider instanceLeaseProvider;

        public DelayedInstanceWrap(Instance instance,
                                   IIntercomMessageHub intercomMessageHub,
                                   IBallotGenerator ballotGenerator,
                                   ISynodConfigurationProvider synodConfigProvider,
                                   InstanceLeaseProviderConfiguration leaseConfig,
                                   ILogger logger)
        {
            this.intercomMessageHub = intercomMessageHub;
            this.ballotGenerator = ballotGenerator;
            this.synodConfigProvider = synodConfigProvider;
            this.leaseConfig = leaseConfig;
            this.logger = logger;
            Task.Delay(leaseConfig.ClockDrift).ContinueWith(_ => CreateInstanceLeaseProvider(instance));
        }

        private void CreateInstanceLeaseProvider(Instance instance)
        {
            var tmp = new InstanceLeaseProvider(instance,
                                                new InstanceRoundBasedRegister(instance,
                                                                               intercomMessageHub,
                                                                               ballotGenerator,
                                                                               synodConfigProvider,
                                                                               leaseConfig,
                                                                               logger),
                                                ballotGenerator,
                                                leaseConfig,
                                                synodConfigProvider,
                                                logger);
            Interlocked.Exchange(ref instanceLeaseProvider, tmp);
        }

        internal IInstanceLeaseProvider InstanceLeaseProvider
        {
            get
            {
                InstanceLeaseProvider tmp = null;
                Interlocked.Exchange(ref tmp, instanceLeaseProvider);

                return tmp;
            }
        }
    }
}