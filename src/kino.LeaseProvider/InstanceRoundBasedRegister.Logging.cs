using kino.Consensus;
using kino.Core.Framework;

namespace kino.LeaseProvider
{
    public partial class InstanceRoundBasedRegister
    {
        private void LogNackRead(Ballot ballot)
        {
            if (writeBallot >= ballot)
            {
                logger.Debug($"process {synodConfigProvider.LocalNode.Uri} " +
                             "NACK_READ ==WB== " +
                             $"{writeBallot.Timestamp:HH:mm:ss fff}-" +
                             $"{writeBallot.MessageNumber}-" +
                             $"{writeBallot.Identity.GetAnyString()} " +
                             ">= " +
                             $"{ballot.Timestamp:HH:mm:ss fff}-" +
                             $"{ballot.MessageNumber}-" +
                             $"{ballot.Identity.GetAnyString()}");
            }
            if (readBallot >= ballot)
            {
                logger.Debug($"process {synodConfigProvider.LocalNode.Uri} " +
                             "NACK_READ ==RB== " +
                             $"{readBallot.Timestamp:HH:mm:ss fff}-" +
                             $"{readBallot.MessageNumber}-" +
                             $"{readBallot.Identity.GetAnyString()} " +
                             ">= " +
                             $"{ballot.Timestamp:HH:mm:ss fff}-" +
                             $"{ballot.MessageNumber}-" +
                             $"{ballot.Identity.GetAnyString()}");
            }
        }

        private void LogAckRead(Ballot ballot)
        {
            if (writeBallot < ballot)
            {
                logger.Debug($"process {synodConfigProvider.LocalNode.Uri} " +
                             "ACK_READ ==WB== " +
                             $"{writeBallot.Timestamp:HH: mm:ss fff}-" +
                             $"{writeBallot.MessageNumber}-" +
                             $"{writeBallot.Identity.GetAnyString()} " +
                             "< " +
                             $"{ballot.Timestamp:HH:mm:ss fff}-" +
                             $"{ballot.MessageNumber}-" +
                             $"{ballot.Identity.GetAnyString()}");
            }
            if (readBallot < ballot)
            {
                logger.Debug($"process {synodConfigProvider.LocalNode.Uri} " +
                             "ACK_READ ==RB== " +
                             $"{readBallot.Timestamp:HH: mm:ss fff}-" +
                             $"{readBallot.MessageNumber}-" +
                             $"{readBallot.Identity.GetAnyString()} " +
                             "< " +
                             $"{ballot.Timestamp:HH:mm:ss fff}-" +
                             $"{ballot.MessageNumber}-" +
                             $"{ballot.Identity.GetAnyString()}");
            }
        }

        private void LogNackWrite(Ballot ballot)
        {
            if (writeBallot > ballot)
            {
                logger.Debug($"process {synodConfigProvider.LocalNode.Uri} " +
                             "NACK_WRITE ==WB== " +
                             $"{writeBallot.Timestamp:HH:mm:ss fff}-" +
                             $"{writeBallot.MessageNumber}-" +
                             $"{writeBallot.Identity.GetAnyString()} " +
                             "> " +
                             $"{ballot.Timestamp:HH:mm:ss fff}-" +
                             $"{ballot.MessageNumber}-" +
                             $"{ballot.Identity.GetAnyString()}");
            }
            if (readBallot > ballot)
            {
                logger.Debug($"process {synodConfigProvider.LocalNode.Uri} " +
                             "NACK_WRITE ==RB== " +
                             $"{readBallot.Timestamp:HH:mm:ss fff}-" +
                             $"{readBallot.MessageNumber}-" +
                             $"{readBallot.Identity.GetAnyString()} " +
                             "> " +
                             $"{ballot.Timestamp:HH:mm:ss fff}-" +
                             $"{ballot.MessageNumber}-" +
                             $"{ballot.Identity.GetAnyString()}");
            }
        }

        private void LogAckWrite(Ballot ballot)
        {
            if (writeBallot <= ballot)
            {
                logger.Debug($"process {synodConfigProvider.LocalNode.Uri} " +
                             "ACK_WRITE ==WB== " +
                             $"{writeBallot.Timestamp:HH:mm:ss fff}-" +
                             $"{writeBallot.MessageNumber}-" +
                             $"{writeBallot.Identity.GetAnyString()} " +
                             "<= " +
                             $"{ballot.Timestamp:HH:mm:ss fff}-" +
                             $"{ballot.MessageNumber}-" +
                             $"{ballot.Identity.GetAnyString()}");
            }
            if (readBallot <= ballot)
            {
                logger.Debug($"process {synodConfigProvider.LocalNode.Uri} " +
                             "ACK_WRITE ==RB== " +
                             $"{readBallot.Timestamp:HH: mm:ss fff}-" +
                             $"{readBallot.MessageNumber}-" +
                             $"{readBallot.Identity.GetAnyString()} " +
                             "<= " +
                             $"{ballot.Timestamp:HH:mm:ss fff}-" +
                             $"{ballot.MessageNumber}-" +
                             $"{ballot.Identity.GetAnyString()}");
            }
        }
    }
}