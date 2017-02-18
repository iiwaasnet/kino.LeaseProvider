using kino.Messaging;
using ProtoBuf;

namespace kino.LeaseProvider.Messages
{
    [ProtoContract]
    internal class InternalDiscoverLeaseProviderInstancesRequestMessage : Payload
    {
        private const ushort MessageVersion = Contract.Version;
        private static readonly byte[] MessageIdentity = "INT-DISCOVERLPINSTREQ".BuildFullIdentity();

        public override ushort Version => MessageVersion;

        public override byte[] Identity => MessageIdentity;
    }
}