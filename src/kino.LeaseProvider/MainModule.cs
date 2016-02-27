using System.Collections.Generic;
using Autofac;
using kino.Actors;
using kino.Consensus;
using kino.Consensus.Configuration;
using kino.Core.Connectivity;
using kino.Core.Diagnostics;
using kino.Core.Messaging;
using kino.Core.Sockets;
using kino.LeaseProvider.Configuration;
using TypedConfigProvider;
using SynodConfiguration = kino.Consensus.Configuration.SynodConfiguration;

namespace kino.LeaseProvider
{
    public class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => new Logger("default"))
                   .As<ILogger>()
                   .SingleInstance();

            RegisterConfiguration(builder);
            RegisterConsensus(builder);

            builder.RegisterType<Composer>()
                   .AsSelf()
                   .SingleInstance();

            builder.Register(c => c.Resolve<Composer>().BuildMessageRouter(c.Resolve<RouterConfiguration>(),
                                                                           c.Resolve<ClusterMembershipConfiguration>(),
                                                                           c.Resolve<IEnumerable<RendezvousEndpoint>>(),
                                                                           c.Resolve<ILogger>()))
                   .As<IMessageRouter>()
                   .SingleInstance();

            builder.Register(c => c.Resolve<Composer>().BuildActorHostManager(c.Resolve<RouterConfiguration>(),
                                                                              c.Resolve<ILogger>()))
                   .As<IActorHostManager>()
                   .SingleInstance();

            builder.Register(c => new SocketFactory(c.ResolveOptional<SocketConfiguration>()))
                   .As<ISocketFactory>()
                   .SingleInstance();

            builder.RegisterType<LeaseProviderService>()
                   .As<ILeaseProviderService>()
                   .SingleInstance();

            builder.RegisterType<LeaseProviderActor>()
                   .As<IActor>()
                   .SingleInstance();

            builder.Register(c => new ProtobufMessageSerializer())
                   .As<IMessageSerializer>()
                   .SingleInstance();
        }

        private static void RegisterConsensus(ContainerBuilder builder)
        {
            builder.RegisterType<IntercomMessageHub>()
                   .As<IIntercomMessageHub>()
                   .SingleInstance();

            builder.RegisterType<RoundBasedRegister>()
                   .As<IRoundBasedRegister>()
                   .SingleInstance();

            builder.RegisterType<BallotGenerator>()
                   .As<IBallotGenerator>()
                   .SingleInstance();

            builder.RegisterType<SynodConfigurationProvider>()
                   .As<ISynodConfigurationProvider>()
                   .SingleInstance();

            builder.RegisterType<SynodConfiguration>()
                   .As<ISynodConfiguration>()
                   .SingleInstance();

            builder.RegisterType<LeaseProvider>()
                   .As<ILeaseProvider>()
                   .SingleInstance();
        }

        private static void RegisterConfiguration(ContainerBuilder builder)
        {
            builder.RegisterType<AppConfigTargetProvider>()
                   .As<IConfigTargetProvider>()
                   .SingleInstance();

            builder.RegisterType<ConfigProvider>()
                   .As<IConfigProvider>()
                   .SingleInstance();

            builder.Register(c => c.Resolve<IConfigProvider>().GetConfiguration<ApplicationConfiguration>())
                   .As<ApplicationConfiguration>()
                   .SingleInstance();

            builder.RegisterType<ConfigurationProvider>()
                   .As<IConfigurationProvider>()
                   .SingleInstance();

            builder.Register(c => c.Resolve<IConfigurationProvider>().GetRouterConfiguration())
                   .As<RouterConfiguration>()
                   .SingleInstance();

            builder.Register(c => c.Resolve<IConfigurationProvider>().GetClusterMembershipConfiguration())
                   .As<ClusterMembershipConfiguration>()
                   .SingleInstance();

            builder.Register(c => c.Resolve<IConfigurationProvider>().GetSynodConfiguration())
                   .As<Configuration.SynodConfiguration>()
                   .SingleInstance();

            builder.Register(c => c.Resolve<IConfigurationProvider>().GetLeaseConfiguration())
                   .As<LeaseConfiguration>()
                   .SingleInstance();

            builder.Register(c => c.Resolve<IConfigurationProvider>().GetRendezvousEndpointsConfiguration())
                   .As<IEnumerable<RendezvousEndpoint>>()
                   .SingleInstance();
        }
    }
}