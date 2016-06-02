using System;
using System.Collections.Generic;
using System.Linq;
using kino.Client;
using kino.Consensus.Configuration;
using kino.Core.Connectivity;

namespace kino.LeaseProvider.Configuration
{
    public class ConfigurationProvider : IConfigurationProvider
    {
        private readonly ApplicationConfiguration appConfig;

        public ConfigurationProvider(ApplicationConfiguration appConfig)
        {
            this.appConfig = appConfig;
        }

        public IEnumerable<RendezvousEndpoint> GetRendezvousEndpointsConfiguration()
            => appConfig.Kino.RendezvousServers.Select(s => new RendezvousEndpoint(s.UnicastUri, s.BroadcastUri));

        public RouterConfiguration GetRouterConfiguration()
            => new RouterConfiguration
               {
                   RouterAddress = new SocketEndpoint(appConfig.Kino.RouterUri),
                   ScaleOutAddress = new SocketEndpoint(appConfig.Kino.ScaleOutAddressUri)
               };

        public ClusterMembershipConfiguration GetClusterMembershipConfiguration()
            => new ClusterMembershipConfiguration
               {
                   PongSilenceBeforeRouteDeletion = appConfig.Kino.PongSilenceBeforeRouteDeletion,
                   PingSilenceBeforeRendezvousFailover = appConfig.Kino.PingSilenceBeforeRendezvousFailover,
                   RunAsStandalone = appConfig.Kino.RunAsStandalone
               };

        public LeaseTimingConfiguration GetLeaseTimingConfiguration()
            => appConfig.LeaseProvider.LeaseTiming;

        public SynodConfiguration GetSynodConfiguration()
            => appConfig.LeaseProvider.Synod;

        public LeaseProviderConfiguration GetLeaseProviderConfiguration()
            => appConfig.LeaseProvider;

        public LeaseConfiguration GetLeaseConfiguration()
            => new LeaseConfiguration
               {
                   ClockDrift = appConfig.LeaseProvider.LeaseTiming.ClockDrift,
                   MaxLeaseTimeSpan = appConfig.LeaseProvider.LeaseTiming.MinAllowedLeaseTimeSpan,
                   MessageRoundtrip = appConfig.LeaseProvider.LeaseTiming.MessageRoundtrip,
                   NodeResponseTimeout = appConfig.LeaseProvider.LeaseTiming.NodeResponseTimeout
               };

        public MessageHubConfiguration GetMessageHubConfiguration()
            => new MessageHubConfiguration {RouterUri = new Uri(appConfig.Kino.RouterUri)};
    }
}