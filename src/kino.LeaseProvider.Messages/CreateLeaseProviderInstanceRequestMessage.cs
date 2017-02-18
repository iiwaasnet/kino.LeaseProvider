using kino.Messaging;
using ProtoBuf;

namespace kino.LeaseProvider.Messages
{
    [ProtoContract]
    public class CreateLeaseProviderInstanceRequestMessage : Payload
    {
        private const ushort MessageVersion = Contract.Version;
        private static readonly byte[] MessageIdentity = "CREATELPINSTREQ".BuildFullIdentity();

        [ProtoMember(1)]
        public string Instance { get; set; }

        public override ushort Version => MessageVersion;

        public override byte[] Identity => MessageIdentity;
    }
}