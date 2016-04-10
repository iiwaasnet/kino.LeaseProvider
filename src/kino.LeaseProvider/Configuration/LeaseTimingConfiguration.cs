using System;

namespace kino.LeaseProvider.Configuration
{
    public class LeaseTimingConfiguration
    {
        public TimeSpan MinAllowedLeaseTimeSpan { get; set; }

        public TimeSpan ClockDrift { get; set; }

        public TimeSpan MessageRoundtrip { get; set; }

        public TimeSpan NodeResponseTimeout { get; set; }
    }
}