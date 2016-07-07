using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Dreamhost.Api;

namespace DnsClientServiceAgent
{
    public class DnsManager
    {
        private static ILog _logger = LogManager.GetLogger(typeof(DnsManager));

        private const string ApiServer = "https://api.dreamhost.com";

        private DreamhostApiClient apiClient;

        public DnsManager(string apikey)
        {
            apiClient = new DreamhostApiClient(ApiServer, apikey);
        }
    }
}
