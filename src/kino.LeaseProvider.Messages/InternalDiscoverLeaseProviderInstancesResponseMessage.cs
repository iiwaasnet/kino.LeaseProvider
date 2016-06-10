using System.Collections.Generic;
using kino.Core.Framework;
using kino.Core.Messaging;
using ProtoBuf;

namespace kino.LeaseProvider.Messages
{
    [ProtoContract]
    public class InternalDiscoverLeaseProviderInstancesResponseMessage : Payload
    {
        private static readonly byte[] MessageVersion = Contract.Version.GetBytes();
        private static readonly byte[] MessageIdentity = "INT-DISCOVERLPINSTRESP".BuildFullIdentity();

        [ProtoMember(1)]
        public IEnumerable<string> Instances { get; set; }

        public override byte[] Version => MessageVersion;

        public override byte[] Identity => MessageIdentity;
    }
}