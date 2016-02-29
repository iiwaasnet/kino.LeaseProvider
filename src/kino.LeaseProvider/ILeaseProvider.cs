using System;
using kino.Consensus;

namespace kino.LeaseProvider
{
    public interface ILeaseProvider
    {
        Lease GetLease(Instance instance, TimeSpan leaseTimeSpan, byte[] requestorIdentity, TimeSpan requestTimeout);
        void Start();
        void Stop();
    }
}