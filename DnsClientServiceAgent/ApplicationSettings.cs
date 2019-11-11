using System.Collections.Generic;

namespace DnsClientServiceAgent
{
    public class ApplicationSettings
    {
        public string ApiKey { get; set; }

        public string RemoteIpCheckUrl { get; set; }

        public List<DomainSetup> Domains { get; set; }
        public int CheckIntervalInMs { get; set; }
    }

    public class DomainSetup
    {
        public string DomainName { get; set; }
        public string @Type { get; set; }
    }
}
