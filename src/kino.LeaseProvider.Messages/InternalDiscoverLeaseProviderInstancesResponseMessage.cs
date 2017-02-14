using System.Collections.Generic;
using kino.Core.Framework;
using kino.Messaging;
using ProtoBuf;

namespace kino.LeaseProvider.Messages
{
    [ProtoContract]
    public class InternalDiscoverLeaseProviderInstancesResponseMessage : Payload
    {
        private static readonly ushort MessageVersion = Contract.Version;
        private static readonly byte[] MessageIdentity = "INT-DISCOVERLPINSTRESP".BuildFullIdentity();

        [ProtoMember(1)]
        public IEnumerable<string> Instances { get; set; }

        public override ushort Version => MessageVersion;

        public override byte[] Identity => MessageIdentity;
    }
}