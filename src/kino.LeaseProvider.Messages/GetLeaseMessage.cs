using kino.Core.Framework;
using kino.Core.Messaging;

namespace kino.LeaseProvider.Messages
{
    public class GetLeaseMessage : Payload
    {
        private static readonly byte[] MessageVersion = "1.0".GetBytes();
        private static readonly byte[] MessageIdentity = "LP.GETLEASE".GetBytes();

        public override byte[] Version => MessageVersion;

        public override byte[] Identity => MessageIdentity;
    }
}