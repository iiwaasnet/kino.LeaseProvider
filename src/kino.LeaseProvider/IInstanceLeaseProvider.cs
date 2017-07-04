using System;
using kino.Consensus;

namespace kino.LeaseProvider
{
    public interface IInstanceLeaseProvider : IDisposable
    {
        Lease GetLease(GetLeaseRequest request);

        bool IsConsensusReached();

        bool IsInstanceStale();
    }
}