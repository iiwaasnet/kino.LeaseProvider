using System;
using ProtoBuf;

namespace kino.LeaseProvider.Messages
{
    [ProtoContract]
    public class Lease
    {
        [ProtoMember(1)]
        public string Instance { get; set; }

        [ProtoMember(2)]
        public Node Owner { get; set; }

        [ProtoMember(3)]
        public DateTime ExpiresAt { get; set; }
    }
}