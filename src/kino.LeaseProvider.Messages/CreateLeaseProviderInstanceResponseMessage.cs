using System;
using kino.Messaging;
using ProtoBuf;

namespace kino.LeaseProvider.Messages
{
    [ProtoContract]
    public class CreateLeaseProviderInstanceResponseMessage : Payload
    {
        private static readonly ushort MessageVersion = Contract.Version;
        private static readonly byte[] MessageIdentity = "CREATELPINSTRESP".BuildFullIdentity();

        [ProtoMember(1)]
        public string Instance { get; set; }

        [ProtoMember(2)]
        public TimeSpan ActivationWaitTime { get; set; }

        public override ushort Version => MessageVersion;

        public override byte[] Identity => MessageIdentity;
    }
}