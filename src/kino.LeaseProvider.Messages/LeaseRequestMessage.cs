using System;
using kino.Messaging;
using ProtoBuf;

namespace kino.LeaseProvider.Messages
{
    [ProtoContract]
    public class LeaseRequestMessage : Payload
    {
        private const ushort MessageVersion = Contract.Version;
        private static readonly byte[] MessageIdentity = "LEASEREQ".BuildFullIdentity();

        [ProtoMember(1)]
        public string Instance { get; set; }

        [ProtoMember(2)]
        public Node Requestor { get; set; }

        [ProtoMember(3)]
        public TimeSpan LeaseTimeSpan { get; set; }

        public override ushort Version => MessageVersion;

        public override byte[] Identity => MessageIdentity;
    }
}