using kino.Messaging;
using ProtoBuf;

namespace kino.LeaseProvider.Messages
{
    [ProtoContract]
    internal class InternalCreateLeaseProviderInstanceRequestMessage : Payload
    {
        private const ushort MessageVersion = Contract.Version;
        private static readonly byte[] MessageIdentity = "INT-CREATELPINSTREQ".BuildFullIdentity();

        [ProtoMember(1)]
        public string Instance { get; set; }

        public override ushort Version => MessageVersion;

        public override byte[] Identity => MessageIdentity;
    }
}