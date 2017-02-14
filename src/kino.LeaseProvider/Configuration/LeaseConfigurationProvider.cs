using kino.Configuration;
using kino.Consensus.Configuration;

namespace kino.LeaseProvider.Configuration
{
    public class LeaseConfigurationProvider : ConfigurationProvider, ILeaseConfigurationProvider
    {
        private readonly LeaseProviderServiceConfiguration appConfig;

        public LeaseConfigurationProvider(LeaseProviderServiceConfiguration appConfig)
            : base(appConfig.Kino)
        {
            this.appConfig = appConfig;
        }

        public SynodConfiguration GetSynodConfiguration()
            => appConfig.LeaseProvider.Synod;

        public LeaseProviderConfiguration GetLeaseProviderConfiguration()
            => appConfig.LeaseProvider;

        public LeaseConfiguration GetLeaseConfiguration()
            => new LeaseConfiguration
               {
                   ClockDrift = appConfig.LeaseProvider.Lease.ClockDrift,
                   MaxLeaseTimeSpan = appConfig.LeaseProvider.Lease.MaxLeaseTimeSpan,
                   MessageRoundtrip = appConfig.LeaseProvider.Lease.MessageRoundtrip,
                   NodeResponseTimeout = appConfig.LeaseProvider.Lease.NodeResponseTimeout
               };
    }
}