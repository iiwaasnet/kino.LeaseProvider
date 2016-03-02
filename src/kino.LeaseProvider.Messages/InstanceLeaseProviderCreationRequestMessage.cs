using System;
using kino.Core.Framework;
using kino.Core.Messaging;
using ProtoBuf;

namespace kino.LeaseProvider.Messages
{
    [ProtoContract]
    public class InstanceLeaseProviderCreationRequestMessage : Payload
    {
        private static readonly byte[] MessageVersion = "1.0".GetBytes();
        private static readonly byte[] MessageIdentity = "LP.INSTLPCREATEREQ".GetBytes();

        [ProtoMember(1)]
        public string Instance { get; set; }

        [ProtoMember(2)]
        public TimeSpan MaxAllowedLeaseTimeSpan { get; set; }

        public override byte[] Version => MessageVersion;

        public override byte[] Identity => MessageIdentity;
    }
}