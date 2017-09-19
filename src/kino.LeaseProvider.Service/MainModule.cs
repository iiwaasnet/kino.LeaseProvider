using Autofac;
using kino.Configuration;
using kino.Core.Diagnostics;
using kino.LeaseProvider.Configuration;
using kino.Security;
using TypedConfigProvider;

namespace kino.LeaseProvider.Service
{
    public class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConfigProvider>()
                   .As<IConfigProvider>()
                   .SingleInstance();

            builder.RegisterType<AppConfigTargetProvider>()
                   .As<IConfigTargetProvider>()
                   .SingleInstance();

            builder.Register(c => c.Resolve<IConfigProvider>().GetConfiguration<LeaseProviderServiceConfiguration>())
                   .As<LeaseProviderServiceConfiguration>()
                   .SingleInstance();

            builder.Register(c => c.Resolve<LeaseProviderServiceConfiguration>().Kino)
                   .As<KinoConfiguration>()
                   .SingleInstance();

            builder.Register(c => c.Resolve<LeaseProviderServiceConfiguration>().Kino.Socket)
                   .AsSelf()
                   .SingleInstance();

            builder.Register(c => new Logger("default"))
                   .As<ILogger>()
                   .SingleInstance();

            builder.Register(c => new DependencyResolver(c))
                   .As<IDependencyResolver>()
                   .SingleInstance();

            builder.Register(c => new LeaseProviderService(c.Resolve<IDependencyResolver>()))
                   .AsSelf()
                   .SingleInstance();

            builder.RegisterType<NullSecurityProvider>()
                   .As<ISecurityProvider>()
                   .SingleInstance();
        }
    }
}