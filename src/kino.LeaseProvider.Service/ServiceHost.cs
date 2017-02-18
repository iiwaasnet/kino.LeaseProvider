using System;
using WindowsServiceHost;
using Autofac;

namespace kino.LeaseProvider.Service
{
    public class ServiceHost : WindowsService
    {
        private LeaseProviderService leaseProviderService;
        private static readonly TimeSpan StartTimeout = TimeSpan.FromSeconds(3);

        protected override ServiceConfiguration GetServiceConfiguration()
            => new ServiceConfiguration
               {
                   ServiceName = "kino.LeaseProvider.Service",
                   DisplayName = "kino.LeaseProvider.Service",
                   OnStart = Start,
                   OnStop = Stop
               };

        private void Start()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<MainModule>();
            var container = builder.Build();

            leaseProviderService = container.Resolve<LeaseProviderService>();
            if (!leaseProviderService.Start(StartTimeout))
            {
                throw new Exception($"Failed starting LeaseProvider after {StartTimeout.TotalMilliseconds} ms!");
            }
        }

        private void Stop()
            => leaseProviderService?.Stop();
    }
}