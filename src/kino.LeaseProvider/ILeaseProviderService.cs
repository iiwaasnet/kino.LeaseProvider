using System;

namespace kino.LeaseProvider
{
    public interface ILeaseProviderService
    {
        bool Start(TimeSpan startTimeout);

        void Stop();
    }
}