using kino.Core.Framework;
using kino.Core.Messaging;
using ProtoBuf;

namespace kino.LeaseProvider.Messages
{
    [ProtoContract]
    public class LeaseResponseMessage : Payload
    {
        private static readonly byte[] MessageVersion = "1.0".GetBytes();
        private static readonly byte[] MessageIdentity = "LEASERESP".BuildFullIdentity();

        [ProtoMember(1)]
        public bool LeaseAquired { get; set; }

        [ProtoMember(2)]
        public Lease Lease { get; set; }

        public override byte[] Version => MessageVersion;

        public override byte[] Identity => MessageIdentity;
    }
}