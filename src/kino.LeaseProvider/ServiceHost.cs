using WindowsServiceHost;
using Autofac;

namespace kino.LeaseProvider
{
    public class ServiceHost : WindowsService
    {
        private ILeaseProviderService leaseProviderService;

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
            var container = builder.Build();

            leaseProviderService = container.Resolve<ILeaseProviderService>();

            leaseProviderService.Start();
        }

        private void Stop()
            => leaseProviderService?.Stop();
    }
}