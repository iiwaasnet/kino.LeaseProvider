using kino.Consensus;

namespace kino.LeaseProvider
{
    public interface ILeaseProvider
    {
        Lease GetLease(Instance instance, byte[] ownerPayload);
    }
}