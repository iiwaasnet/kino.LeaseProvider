using System.Collections.Generic;
using System.Linq;
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

        public LeaseConfiguration GetLeaseConfiguration()
            => appConfig.LeaseProvider.Lease;

        public SynodConfiguration GetSynodConfiguration()
            => appConfig.LeaseProvider.Synod;
    }
}