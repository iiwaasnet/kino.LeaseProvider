using kino.Actors.Diagnostics;
using kino.Configuration;
using kino.Connectivity;
using kino.Consensus;
using kino.Consensus.Configuration;
using kino.Core.Diagnostics;
using kino.Core.Diagnostics.Performance;
using kino.LeaseProvider.Actors;
using kino.LeaseProvider.Configuration;
using kino.Messaging;

namespace kino.LeaseProvider
{
    public partial class LeaseProviderService
    {

        public void Build()
        {
            var logger = resolver.Resolve<ILogger>();
            var applicationConfig = resolver.Resolve<LeaseProviderServiceConfiguration>();
            var configurationProvider = new ConfigurationProvider(applicationConfig.Kino);
            var socketConfiguration = configurationProvider.GetSocketConfiguration();
            var messageWireFormatter =
#if NET47
                resolver.Resolve<IMessageWireFormatter>() ?? new MessageWireFormatterV5();
#else
            resolver.Resolve<IMessageWireFormatter>() ?? new MessageWireFormatterV6_1();
#endif
            var socketFactory = new SocketFactory(messageWireFormatter, socketConfiguration);
            var synodConfigProvider = new SynodConfigurationProvider(applicationConfig.LeaseProvider.Synod);
#if NET47
            var instanceNameResolver = resolver.Resolve<IInstanceNameResolver>() ?? new InstanceNameResolver();
            var performanceCounterManager = new PerformanceCounterManager<KinoPerformanceCounters>(instanceNameResolver, logger);
#else
            var performanceCounterManager = default(IPerformanceCounterManager<KinoPerformanceCounters>);
#endif
            var intercomMessageHub = new IntercomMessageHub(socketFactory,
                                                            synodConfigProvider,
                                                            performanceCounterManager,
                                                            logger);
            var ballotGenerator = new BallotGenerator(applicationConfig.LeaseProvider.Lease);
            kino = new kino(resolver);
            messageHub = kino.GetMessageHub();
            leaseProvider = new LeaseProvider(intercomMessageHub,
                                              ballotGenerator,
                                              synodConfigProvider,
                                              applicationConfig.LeaseProvider.Lease,
                                              applicationConfig.LeaseProvider,
                                              messageHub,
                                              logger);
            var serializer = new ProtobufMessageSerializer();
            kino.AssignActor(new LeaseProviderActor(leaseProvider, serializer, applicationConfig.LeaseProvider, logger));
            kino.AssignActor(new InstanceDiscoveryActor(leaseProvider, applicationConfig.LeaseProvider));
            kino.AssignActor(new InstanceBuilderActor(leaseProvider, applicationConfig.LeaseProvider));
            kino.AssignActor(new ExceptionHandlerActor(logger));
        }
    }
}