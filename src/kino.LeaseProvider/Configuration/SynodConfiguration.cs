using System;
using System.Collections.Generic;

namespace kino.LeaseProvider.Configuration
{
    public class SynodConfiguration
    {
        public Uri LocalNode { get; set; }

        public IEnumerable<Uri> Members { get; set; }
    }
}