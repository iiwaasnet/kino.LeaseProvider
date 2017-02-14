using kino.Core.Framework;
using kino.Messaging;
using ProtoBuf;

namespace kino.LeaseProvider.Messages
{
    [ProtoContract]
    public class InternalDiscoverLeaseProviderInstancesRequestMessage : Payload
    {
        private static readonly ushort MessageVersion = Contract.Version;
        private static readonly byte[] MessageIdentity = "INT-DISCOVERLPINSTREQ".BuildFullIdentity();

        public override ushort Version => MessageVersion;

        public override byte[] Identity => MessageIdentity;
    }
}