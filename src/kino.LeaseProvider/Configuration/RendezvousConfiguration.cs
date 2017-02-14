using System;

namespace kino.LeaseProvider.Configuration
{
    public class RendezvousConfiguration
    {
        public Uri BroadcastUri { get; set; }

        public Uri UnicastUri { get; set; }

        public TimeSpan HeartBeatInterval { get; set; }
    }
}