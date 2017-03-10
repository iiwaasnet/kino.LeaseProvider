using System.Collections.Generic;

namespace kino.LeaseProvider.Configuration
{
    public class SynodConfiguration
    {
        public string LocalNode { get; set; }

        public IEnumerable<string> Members { get; set; }
    }
}