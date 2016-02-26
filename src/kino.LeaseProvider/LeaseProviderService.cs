namespace kino.LeaseProvider
{
    public class LeaseProviderService : ILeaseProviderService
    {
        private readonly ILeaseProvider leaseProvider;

        public LeaseProviderService(ILeaseProvider leaseProvider)
        {
            this.leaseProvider = leaseProvider;
        }

        public void Start()
        {
            leaseProvider.Start();
        }

        public void Stop()
        {
            leaseProvider.Stop();
        }
    }
}