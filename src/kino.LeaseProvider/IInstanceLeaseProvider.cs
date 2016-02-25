using kino.Consensus;

namespace kino.LeaseProvider
{
    public interface IInstanceLeaseProvider
    {
        Lease GetLease(byte[] ownerPayload);
    }
}