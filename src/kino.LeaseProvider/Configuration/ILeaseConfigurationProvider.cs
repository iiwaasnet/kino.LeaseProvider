using kino.Configuration;
using kino.Consensus.Configuration;

namespace kino.LeaseProvider.Configuration
{
    public interface ILeaseConfigurationProvider : IConfigurationProvider
    {
        LeaseConfiguration GetLeaseConfiguration();

        SynodConfiguration GetSynodConfiguration();

        LeaseProviderConfiguration GetLeaseProviderConfiguration();
    }
}