using kino.Consensus;

namespace kino.LeaseProvider
{
    public interface IInstanceRoundBasedRegister : IRoundBasedRegister
    {
        bool IsInstanceStale();

        bool IsConsensusReached();
    }
}