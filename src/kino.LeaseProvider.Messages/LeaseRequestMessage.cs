using System;
using kino.Core.Framework;
using kino.Core.Messaging;
using ProtoBuf;

namespace kino.LeaseProvider.Messages
{
    [ProtoContract]
    public class LeaseRequestMessage : Payload
    {
        private static readonly byte[] MessageVersion = Contract.Version.GetBytes();
        private static readonly byte[] MessageIdentity = "LEASEREQ".BuildFullIdentity();

        [ProtoMember(1)]
        public string Instance { get; set; }

        [ProtoMember(2)]
        public Node Requestor { get; set; }

        [ProtoMember(3)]
        public TimeSpan LeaseTimeSpan { get; set; }

        public override byte[] Version => MessageVersion;

        public override byte[] Identity => MessageIdentity;
    }
}