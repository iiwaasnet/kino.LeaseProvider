using kino.Consensus.Configuration;

namespace kino.LeaseProvider.Configuration
{
    public class LeaseProviderConfiguration
    {
        public string ClusterName { get; set; }

        public SynodConfiguration Synod { get; set; }

        public LeaseConfiguration Lease { get; set; }
    }
}