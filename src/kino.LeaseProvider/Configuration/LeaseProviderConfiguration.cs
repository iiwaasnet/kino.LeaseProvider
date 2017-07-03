using System;

namespace kino.LeaseProvider.Configuration
{
    public class LeaseProviderConfiguration
    {
        public string ClusterName { get; set; }

        public SynodConfiguration Synod { get; set; }

        public InstanceLeaseProviderConfiguration Lease { get; set; }

        public TimeSpan StaleInstancesCleanupPeriod { get; set; }
    }
}