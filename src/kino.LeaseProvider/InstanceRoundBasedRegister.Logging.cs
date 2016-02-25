using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using kino.Consensus;
using kino.Consensus.Configuration;
using kino.Consensus.Messages;
using kino.Core.Diagnostics;
using kino.Core.Framework;
using kino.Core.Messaging;
using Ballot = kino.Consensus.Ballot;
using Lease = kino.Consensus.Lease;

namespace kino.LeaseProvider
{
    public partial class InstanceRoundBasedRegister
    {
        private void LogNackRead(Ballot ballot)
        {
            if (writeBallot >= ballot)
            {
                logger.Debug($"process {synodConfig.LocalNode.Uri.AbsoluteUri} " +
                             "NACK_READ ==WB== " +
                             $"{writeBallot.Timestamp.ToString("HH:mm:ss fff")}-" +
                             $"{writeBallot.MessageNumber}-" +
                             $"{writeBallot.Identity.GetString()} " +
                             ">= " +
                             $"{ballot.Timestamp.ToString("HH:mm:ss fff")}-" +
                             $"{ballot.MessageNumber}-" +
                             $"{ballot.Identity.GetString()}");
            }
            if (readBallot >= ballot)
            {
                logger.Debug($"process {synodConfig.LocalNode.Uri.AbsoluteUri} " +
                             "NACK_READ ==RB== " +
                             $"{readBallot.Timestamp.ToString("HH:mm:ss fff")}-" +
                             $"{readBallot.MessageNumber}-" +
                             $"{readBallot.Identity.GetString()} " +
                             ">= " +
                             $"{ballot.Timestamp.ToString("HH:mm:ss fff")}-" +
                             $"{ballot.MessageNumber}-" +
                             $"{ballot.Identity.GetString()}");
            }
        }

        private void LogAckRead(Ballot ballot)
        {
            if (writeBallot < ballot)
            {
                logger.Debug($"process {synodConfig.LocalNode.Uri.AbsoluteUri} " +
                             "ACK_READ ==WB== " +
                             $"{writeBallot.Timestamp.ToString("HH: mm:ss fff")}-" +
                             $"{writeBallot.MessageNumber}-" +
                             $"{writeBallot.Identity.GetString()} " +
                             "< " +
                             $"{ballot.Timestamp.ToString("HH:mm:ss fff")}-" +
                             $"{ballot.MessageNumber}-" +
                             $"{ballot.Identity.GetString()}");
            }
            if (readBallot < ballot)
            {
                logger.Debug($"process {synodConfig.LocalNode.Uri.AbsoluteUri} " +
                             "ACK_READ ==RB== " +
                             $"{readBallot.Timestamp.ToString("HH: mm:ss fff")}-" +
                             $"{readBallot.MessageNumber}-" +
                             $"{readBallot.Identity.GetString()} " +
                             "< " +
                             $"{ballot.Timestamp.ToString("HH:mm:ss fff")}-" +
                             $"{ballot.MessageNumber}-" +
                             $"{ballot.Identity.GetString()}");
            }
        }

        private void LogNackWrite(Ballot ballot)
        {
            if (writeBallot > ballot)
            {
                logger.Debug($"process {synodConfig.LocalNode.Uri.AbsoluteUri} " +
                             "NACK_WRITE ==WB== " +
                             $"{writeBallot.Timestamp.ToString("HH:mm:ss fff")}-" +
                             $"{writeBallot.MessageNumber}-" +
                             $"{writeBallot.Identity.GetString()} " +
                             "> " +
                             $"{ballot.Timestamp.ToString("HH:mm:ss fff")}-" +
                             $"{ballot.MessageNumber}-" +
                             $"{ballot.Identity.GetString()}");
            }
            if (readBallot > ballot)
            {
                logger.Debug($"process {synodConfig.LocalNode.Uri.AbsoluteUri} " +
                             "NACK_WRITE ==RB== " +
                             $"{readBallot.Timestamp.ToString("HH:mm:ss fff")}-" +
                             $"{readBallot.MessageNumber}-" +
                             $"{readBallot.Identity.GetString()} " +
                             "> " +
                             $"{ballot.Timestamp.ToString("HH:mm:ss fff")}-" +
                             $"{ballot.MessageNumber}-" +
                             $"{ballot.Identity.GetString()}");
            }
        }

        private void LogAckWrite(Ballot ballot)
        {
            if (writeBallot <= ballot)
            {
                logger.Debug($"process {synodConfig.LocalNode.Uri.AbsoluteUri} " +
                             "ACK_WRITE ==WB== " +
                             $"{writeBallot.Timestamp.ToString("HH:mm:ss fff")}-" +
                             $"{writeBallot.MessageNumber}-" +
                             $"{writeBallot.Identity.GetString()} " +
                             "<= " +
                             $"{ballot.Timestamp.ToString("HH:mm:ss fff")}-" +
                             $"{ballot.MessageNumber}-" +
                             $"{ballot.Identity.GetString()}");
            }
            if (readBallot <= ballot)
            {
                logger.Debug($"process {synodConfig.LocalNode.Uri.AbsoluteUri} " +
                             "ACK_WRITE ==RB== " +
                             $"{readBallot.Timestamp.ToString("HH: mm:ss fff")}-" +
                             $"{readBallot.MessageNumber}-" +
                             $"{readBallot.Identity.GetString()} " +
                             "<= " +
                             $"{ballot.Timestamp.ToString("HH:mm:ss fff")}-" +
                             $"{ballot.MessageNumber}-" +
                             $"{ballot.Identity.GetString()}");
            }
        }
    }
}