using System;

namespace kino.LeaseProvider
{
    public class GetLeaseRequest
    {
        public byte[] RequestorIdentity { get; set; }

        public TimeSpan LeaseTimeSpan { get; set; }

        public int MinValidityTimeFraction { get; set; }
    }
}