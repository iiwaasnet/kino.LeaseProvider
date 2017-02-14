using System;
using kino.Connectivity;
using kino.Consensus;
using kino.Core.Diagnostics;
using kino.Core.Diagnostics.Performance;
using kino.LeaseProvider.Configuration;
using SynodConfiguration = kino.Consensus.Configuration.SynodConfiguration;

namespace kino.LeaseProvider
{
    public partial class LeaseProviderService : IDisposable
    {
        private kino kino;

        public ILeaseProviderService GetLeaseProviderService(IDependencyResolver resolver)
        {
            var logger = resolver.Resolve<ILogger>();
            var applicationConfig = resolver.Resolve<LeaseProviderServiceConfiguration>();
            var socketFactory = new SocketFactory(resolver.Resolve<SocketConfiguration>());
            var synodConfigProvider = new SynodConfigurationProvider(applicationConfig.LeaseProvider.Synod);
            var synodConfig = new SynodConfiguration(synodConfigProvider);
            var instanceNameResolver = resolver.Resolve<IInstanceNameResolver>() ?? new InstanceNameResolver();
            var performanceCounterManager = new PerformanceCounterManager<KinoPerformanceCounters>(instanceNameResolver,
                                                                                                   logger);
            var intercomMessageHub = new IntercomMessageHub(socketFactory,
                                                            synodConfig,
                                                            performanceCounterManager,
                                                            logger);
            var ballotGenerator = new BallotGenerator(applicationConfig.LeaseProvider.Lease);
            kino = new kino(resolver);
            var leaseProvider = new LeaseProvider(intercomMessageHub,
                                                  ballotGenerator,
                                                  synodConfig,
                                                  applicationConfig.LeaseProvider.Lease,
                                                  applicationConfig.LeaseProvider,
                                                  kino.GetMessageHub(),
                                                  logger);
            var leaseProviderService = new LeaseProviderService(leaseProvider, 
                kino.);
        }

        public void Dispose()
        {
            kino.Dispose();
        }
    }
}