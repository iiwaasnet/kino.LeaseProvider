﻿using Autofac;
using kino.Configuration;
using kino.Core.Diagnostics;
using TypedConfigProvider;

namespace kino.LeaseProvider.Client
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

            builder.Register(c => c.Resolve<IConfigProvider>().GetConfiguration<ApplicationConfiguration>())
                   .As<ApplicationConfiguration>()
                   .SingleInstance();

            builder.Register(c => new Logger("default"))
                   .As<ILogger>()
                   .SingleInstance();

            builder.Register(c => new DependencyResolver(c))
                   .As<IDependencyResolver>()
                   .SingleInstance();

            builder.Register(c => new kino(c.Resolve<IDependencyResolver>()))
                   .AsSelf()
                   .SingleInstance();
        }
    }
}