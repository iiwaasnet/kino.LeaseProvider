using kino.Core.Framework;
using kino.Core.Messaging;
using ProtoBuf;

namespace kino.LeaseProvider.Messages
{
    [ProtoContract]
    public class GetLeaseMessage : Payload
    {
        private static readonly byte[] MessageVersion = "1.0".GetBytes();
        private static readonly byte[] MessageIdentity = "LP.GETLEASE".GetBytes();

        [ProtoMember(1)]
        public string Instance { get; set; }

        [ProtoMember(2)]
        public byte[] RequestorIdentity { get; set; }

        [ProtoMember(3)]
        public string RequestorUri { get; set; }

        public override byte[] Version => MessageVersion;

        public override byte[] Identity => MessageIdentity;
    }
}