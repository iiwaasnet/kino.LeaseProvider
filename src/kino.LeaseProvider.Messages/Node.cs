using ProtoBuf;

namespace kino.LeaseProvider.Messages
{
    [ProtoContract]
    public class Node
    {
        [ProtoMember(1)]
        public byte[] Identity { get; set; }

        [ProtoMember(2)]
        public string Uri { get; set; }
    }
}