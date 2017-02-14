using kino.Core.Framework;
using kino.Messaging;
using ProtoBuf;

namespace kino.LeaseProvider.Messages
{
    [ProtoContract]
    public class LeaseResponseMessage : Payload
    {
        private static readonly ushort MessageVersion = Contract.Version;
        private static readonly byte[] MessageIdentity = "LEASERESP".BuildFullIdentity();

        [ProtoMember(1)]
        public bool LeaseAquired { get; set; }

        [ProtoMember(2)]
        public Lease Lease { get; set; }

        public override ushort Version => MessageVersion;

        public override byte[] Identity => MessageIdentity;
    }
}