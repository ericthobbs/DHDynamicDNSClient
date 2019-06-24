using System;
using System.Collections.Generic;
using System.Text;
using Dreamhost.Api;

namespace DnsClientServiceAgent
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
        public string ZoneName { get; set; }
        public string @Type { get; set; }
    }
}
