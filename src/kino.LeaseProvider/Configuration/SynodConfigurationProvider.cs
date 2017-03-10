using System;
using System.Collections.Generic;
using System.Linq;
using kino.Consensus.Configuration;
using kino.Core.Framework;

namespace kino.LeaseProvider.Configuration
{
    public class SynodConfigurationProvider : ISynodConfigurationProvider
    {
        public SynodConfigurationProvider(SynodConfiguration config)
        {
            LocalNode = config.LocalNode.ParseAddress();
            Synod = config.Members
                          .Select(m => m.ParseAddress())
                          .ToList();
        }

        public Uri LocalNode { get; }

        public IEnumerable<Uri> Synod { get; }
    }
}