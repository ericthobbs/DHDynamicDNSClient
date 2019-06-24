using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dreamhost.Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DnsClientServiceAgent
{
    public class DynamicDnsUpdateService
    {
        private readonly ILogger<DynamicDnsUpdateService> _logger;
        private readonly IOptionsMonitor<ApplicationSettings> _settings;
        private readonly DreamhostApiClient _client;

        private readonly System.Threading.Timer workTimer;

        public DynamicDnsUpdateService(
            ILoggerFactory loggerFactory, 
            ILogger<DynamicDnsUpdateService> logger,
            IOptionsMonitor<ApplicationSettings> settings,
            DreamhostApiClient client)
        {
            _logger = logger;
            _settings = settings;
            _client = client;
            _client.ApiKey = _settings.CurrentValue.ApiKey;
            workTimer = new Timer(
                OnTimerFired, 
                this,
                Timeout.InfiniteTimeSpan, 
                TimeSpan.FromMilliseconds(-1));
        }

        private void OnTimerFired(object state)
        {
            _logger.LogDebug("Firing....");

            //Work Loop
            
            //Grab dns records
            //check if the dns record we are looking for has a different address
            //delete old record if address differs
            //then add updated record
        }

        public void Start()
        {
            workTimer.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(_settings.CurrentValue.CheckIntervalInMs));
        }

        public void Stop()
        {
            workTimer.Change(TimeSpan.MaxValue, TimeSpan.FromMilliseconds(-1));
        }
    }
}
