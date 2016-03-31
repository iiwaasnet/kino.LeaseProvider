﻿using System;
using kino.Core.Framework;
using kino.Core.Messaging;
using ProtoBuf;

namespace kino.LeaseProvider.Messages
{
    [ProtoContract]
    public class CreateLeaseProviderInstanceResponseMessage : Payload
    {
        private static readonly byte[] MessageVersion = Contract.Version.GetBytes();
        private static readonly byte[] MessageIdentity = "CREATELPINSTRESP".BuildFullIdentity();

        [ProtoMember(1)]
        public string Instance { get; set; }

        [ProtoMember(2)]
        public TimeSpan ActivationWaitTime { get; set; }

        public override byte[] Version => MessageVersion;

        public override byte[] Identity => MessageIdentity;
    }
}