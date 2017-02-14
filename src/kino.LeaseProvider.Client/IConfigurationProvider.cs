using System.Collections.Generic;
using kino.Cluster.Configuration;

namespace kino.LeaseProvider.Client
{
    public interface IConfigurationProvider
    {
        IEnumerable<RendezvousEndpoint> GetRendezvousEndpointsConfiguration();

        RouterConfiguration GetRouterConfiguration();

        ClusterMembershipConfiguration GetClusterMembershipConfiguration();

        MessageHubConfiguration GetMessageHubConfiguration();
    }
}