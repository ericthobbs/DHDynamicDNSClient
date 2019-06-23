using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Dreamhost.Api;

namespace DnsClientServiceAgent
{
    public class DynamicDnsUpdateService
    {
        private static ILog _logger = LogManager.GetLogger(typeof(DynamicDnsUpdateService));

        public const string ApiServer = "https://api.dreamhost.com";
        public const string RemoteScript = "http://scripts.badpointer.net/external_ip.php";

        private DreamhostApiClient apiClient;

        public DynamicDnsUpdateService()
        {
            var apikey = ConfigurationManager.AppSettings["apikey"];
            var apiserver = ConfigurationManager.AppSettings["apiserver"];
            var remoteipscript = ConfigurationManager.AppSettings["remoteipscript"];

            apiClient = new DreamhostApiClient(apiserver ?? ApiServer, apikey);
        }

        public void Start()
        {
            
        }

        public void Stop()
        {
            
        }
    }
}
