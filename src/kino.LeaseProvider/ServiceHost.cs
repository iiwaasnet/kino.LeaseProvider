using System;
using WindowsServiceHost;
using Autofac;
using Autofac.kino;

namespace kino.LeaseProvider
{
    public class ServiceHost : WindowsService
    {
        private ILeaseProviderService leaseProviderService;
        private static readonly TimeSpan StartTimeout = TimeSpan.FromSeconds(3);

        protected override ServiceConfiguration GetServiceConfiguration()
            => new ServiceConfiguration
               {
                   ServiceName = "kino.LeaseProvider",
                   DisplayName = "kino.LeaseProvider",
                   OnStart = Start,
                   OnStop = Stop
               };

        private void Start()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<MainModule>();
            builder.RegisterModule<KinoModule>();
            var container = builder.Build();

            leaseProviderService = container.Resolve<ILeaseProviderService>();
            if (!leaseProviderService.Start(StartTimeout))
            {
                throw new Exception($"Failed starting LeaseProvider after {StartTimeout.TotalMilliseconds} ms!");
            }
        }

        private void Stop()
            => leaseProviderService?.Stop();
    }
}