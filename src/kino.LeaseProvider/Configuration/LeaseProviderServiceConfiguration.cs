using kino.Configuration;

namespace kino.LeaseProvider.Configuration
{
    public class LeaseProviderServiceConfiguration
    {
        public KinoConfiguration Kino { get; set; }

        public LeaseProviderConfiguration LeaseProvider { get; set; }
    }
}