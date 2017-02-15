using kino.Connectivity;
using kino.Consensus;
using kino.Core.Diagnostics;
using kino.Core.Diagnostics.Performance;
using kino.LeaseProvider.Actors;
using kino.LeaseProvider.Configuration;
using kino.Messaging;
using SynodConfiguration = kino.Consensus.Configuration.SynodConfiguration;

namespace kino.LeaseProvider
{
    public partial class LeaseProviderService
    {
        private void Build()
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
            leaseProvider = new LeaseProvider(intercomMessageHub,
                                              ballotGenerator,
                                              synodConfig,
                                              applicationConfig.LeaseProvider.Lease,
                                              applicationConfig.LeaseProvider,
                                              kino.GetMessageHub(),
                                              logger);
            var serializer = new ProtobufMessageSerializer();
            kino.AssignActor(new LeaseProviderActor(leaseProvider, serializer, applicationConfig.LeaseProvider));
            kino.AssignActor(new InstanceDiscoveryActor(leaseProvider, serializer, applicationConfig.LeaseProvider));
            kino.AssignActor(new InstanceBuilderActor(leaseProvider, serializer, applicationConfig.LeaseProvider));
        }
    }
}