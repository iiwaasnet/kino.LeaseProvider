using System;

namespace kino.LeaseProvider
{
    internal class InstanceLeaseProviderBag
    {
        internal IInstanceLeaseProvider InstanceLeaseProvider { get; set; }

        internal TimeSpan MaxAllowedLeaseTimeSpan { get; set; }
    }
}