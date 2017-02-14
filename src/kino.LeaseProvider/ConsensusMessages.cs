﻿using kino.Consensus.Messages;
using kino.Core;

namespace kino.LeaseProvider
{
    internal class ConsensusMessages
    {
        internal static readonly MessageIdentifier LeaseAckRead = MessageIdentifier.Create<LeaseAckReadMessage>();
        internal static readonly MessageIdentifier LeaseAckWrite = MessageIdentifier.Create<LeaseAckWriteMessage>();
        internal static readonly MessageIdentifier LeaseNackRead = MessageIdentifier.Create<LeaseNackReadMessage>();
        internal static readonly MessageIdentifier LeaseNackWrite = MessageIdentifier.Create<LeaseNackWriteMessage>();
        internal static readonly MessageIdentifier LeaseRead = MessageIdentifier.Create<LeaseReadMessage>();
        internal static readonly MessageIdentifier LeaseWrite = MessageIdentifier.Create<LeaseWriteMessage>();
    }
}