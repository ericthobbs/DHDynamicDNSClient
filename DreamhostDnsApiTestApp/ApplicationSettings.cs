using System.Collections.Generic;

namespace DreamhostDnsApiTestApp
{
    public class ApplicationSettings
    {
        public string ApiKey { get; set; }
        public List<DomainSetup> Domains { get; set; }
        public int CheckIntervalInMs { get; set; }
    }

    public class DomainSetup
    {
        public string DomainName { get; set; }
        public string Zone { get; set; }
        public string @Type { get; set; }
    }
}
