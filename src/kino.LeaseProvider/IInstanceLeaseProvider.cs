using System;
using kino.Consensus;

namespace kino.LeaseProvider
{
    public interface IInstanceLeaseProvider
    {
        Lease GetLease(byte[] requestorIdentity, TimeSpan leaseTimeSpan);
    }
}