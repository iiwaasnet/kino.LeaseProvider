using System.Collections.Generic;
using kino.Consensus.Configuration;
using kino.Core.Connectivity;

namespace kino.LeaseProvider.Configuration
{
    public interface IConfigurationProvider
    {
        LeaseConfiguration GetLeaseConfiguration();
        IEnumerable<RendezvousEndpoint> GetRendezvousEndpointsConfiguration();
        RouterConfiguration GetRouterConfiguration();
        ClusterMembershipConfiguration GetClusterMembershipConfiguration();
        SynodConfiguration GetSynodConfiguration();
    }
}