﻿using kino.Core.Framework;
using kino.Core.Messaging;
using ProtoBuf;

namespace kino.LeaseProvider.Messages
{
    [ProtoContract]
    public class InternalCreateLeaseProviderInstanceRequestMessage : Payload
    {
        private static readonly byte[] MessageVersion = Contract.Version.GetBytes();
        private static readonly byte[] MessageIdentity = "INT-CREATELPINSTREQ".BuildFullIdentity();

        [ProtoMember(1)]
        public string Instance { get; set; }

        public override byte[] Version => MessageVersion;

        public override byte[] Identity => MessageIdentity;
    }
}