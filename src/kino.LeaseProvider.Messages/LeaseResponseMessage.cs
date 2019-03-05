using kino.Messaging;
using ProtoBuf;

namespace kino.LeaseProvider.Messages
{
    [ProtoContract]
    public class LeaseResponseMessage : Payload
    {
        private const ushort MessageVersion = Contract.Version;
        private static readonly byte[] MessageIdentity = "LEASERESP".BuildFullIdentity();

        [ProtoMember(1)]
        public bool LeaseAcquired { get; set; }

        [ProtoMember(2)]
        public Lease Lease { get; set; }

        [ProtoMember(3)]
        public bool LeaseIssueFailed { get; set; }

        public override ushort Version => MessageVersion;

        public override byte[] Identity => MessageIdentity;
    }
}