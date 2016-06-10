using kino.Core.Framework;
using kino.Core.Messaging;
using ProtoBuf;

namespace kino.LeaseProvider.Messages
{
    [ProtoContract]
    public class InternalDiscoverLeaseProviderInstancesRequestMessage : Payload
    {
        private static readonly byte[] MessageVersion = Contract.Version.GetBytes();
        private static readonly byte[] MessageIdentity = "INT-DISCOVERLPINSTREQ".BuildFullIdentity();

        public override byte[] Version => MessageVersion;

        public override byte[] Identity => MessageIdentity;
    }
}