using kino.Actors;
using kino.Actors.Diagnostics;
using kino.Consensus;
using kino.Consensus.Configuration;
using kino.LeaseProvider.Actors;
using kino.LeaseProvider.Configuration;
using kino.Messaging;
using ILeaseProvider = kino.LeaseProvider.ILeaseProvider;

namespace Autofac.kino.LeaseProvider
{
    public class LeaseProviderModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<KinoModule>();

            builder.Register(c => c.Resolve<LeaseProviderServiceConfiguration>().LeaseProvider)
                   .AsSelf()
                   .SingleInstance();

            builder.Register(c => c.Resolve<LeaseProviderServiceConfiguration>().LeaseProvider.Synod)
                   .AsSelf()
                   .SingleInstance();

            builder.Register(c => c.Resolve<LeaseProviderServiceConfiguration>().LeaseProvider.Lease)
                   .AsSelf()
                   .As<LeaseConfiguration>()
                   .SingleInstance();

            builder.Register(c => c.Resolve<LeaseProviderServiceConfiguration>().Kino)
                   .AsSelf()
                   .SingleInstance();

            builder.Register(c => c.Resolve<LeaseProviderServiceConfiguration>().Kino.Socket)
                   .AsSelf()
                   .SingleInstance();

            builder.RegisterType<SynodConfigurationProvider>()
                   .As<ISynodConfigurationProvider>()
                   .SingleInstance();

            builder.RegisterType<IntercomMessageHub>()
                   .As<IIntercomMessageHub>()
                   .SingleInstance();

            builder.RegisterType<BallotGenerator>()
                   .As<IBallotGenerator>()
                   .SingleInstance();

            builder.RegisterType<global::kino.LeaseProvider.LeaseProvider>()
                   .As<ILeaseProvider>()
                   .SingleInstance();

            builder.RegisterType<ProtobufMessageSerializer>()
                   .As<IMessageSerializer>()
                   .SingleInstance();

            builder.RegisterType<LeaseProviderActor>()
                   .As<IActor>()
                   .SingleInstance();

            builder.RegisterType<InstanceDiscoveryActor>()
                   .As<IActor>()
                   .SingleInstance();

            builder.RegisterType<InstanceBuilderActor>()
                   .As<IActor>()
                   .SingleInstance();
            builder.RegisterType<ExceptionHandlerActor>()
                   .As<IActor>()
                   .SingleInstance();
        }
    }
}