using System;
using kino.Consensus.Configuration;

namespace kino.LeaseProvider.Configuration
{
    public class InstanceLeaseProviderConfiguration : LeaseConfiguration
    {
        public TimeSpan LeaseProviderIsStaleAfter { get; set; }
    }
}