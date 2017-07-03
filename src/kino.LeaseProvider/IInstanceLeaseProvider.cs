using kino.Consensus;

namespace kino.LeaseProvider
{
    public interface IInstanceLeaseProvider
    {
        Lease GetLease(GetLeaseRequest request);

        bool IsConsensusReached();

        bool IsInstanceStale();
    }
}