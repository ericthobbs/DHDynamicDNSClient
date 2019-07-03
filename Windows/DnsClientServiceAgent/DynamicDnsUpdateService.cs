using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Dreamhost.Api;
using Dreamhost.Api.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DnsClientServiceAgent
{
    public class DynamicDnsUpdateService
    {
        private readonly ILogger<DynamicDnsUpdateService> _logger;
        private readonly IOptionsMonitor<ApplicationSettings> _settings;
        private readonly DreamhostApiClient _client;
        private readonly HttpClient _httpClient;

        private readonly System.Threading.Timer _workTimer;

        public const string RemoteScript = "http://scripts.badpointer.net/external_ip.php";

        public DynamicDnsUpdateService(
            ILoggerFactory loggerFactory, 
            ILogger<DynamicDnsUpdateService> logger,
            IOptionsMonitor<ApplicationSettings> settings,
            DreamhostApiClient client, IHttpClientFactory factory)
        { 
            _logger = logger;
            _settings = settings;
            _client = client;
            _client.ApiKey = _settings.CurrentValue.ApiKey;
            _httpClient = factory.CreateClient();

            _workTimer = new Timer(
                OnTimerFired, 
                this,
                Timeout.InfiniteTimeSpan, 
                TimeSpan.FromMilliseconds(-1));

            GC.KeepAlive(_workTimer);
        }

        private void OnTimerFired(object state)
        {
            void DeleteDnsRecords(List<DnsRecord> records)
            {
                var tasks = new Task[records.Count];
                for (var i = 0; i < records.Count; i++)
                {
                    tasks[i] = _client.DnsRemoveRecord(records[i]);
                }

                try
                {
                    Task.WaitAll(tasks);
                    _logger.LogInformation("All records deleted successfully!");
                }
                catch (AggregateException ex)
                {
                    foreach (var ie in ex.InnerExceptions)
                    {
                        _logger.LogError(ie, "Failed to delete dns record.");
                    }
                }
            }

            void CreateDnsRecords(IPAddress ipAddress)
            {
                Task[] tasks;
                tasks = new Task[_settings.CurrentValue.Domains.Count];
                for (var i = 0; i < _settings.CurrentValue.Domains.Count; i++)
                {
                    tasks[i] = _client.DnsAddRecord(
                        _settings.CurrentValue.Domains[i].DomainName,
                        ipAddress.ToString(),
                        _settings.CurrentValue.Domains[i].Type,
                        $"Managed by Service at {DateTime.Now:R}");
                }

                try
                {
                    Task.WaitAll(tasks);
                    _logger.LogInformation("All Records created successfully!");
                }
                catch (AggregateException ex)
                {
                    foreach (var ie in ex.InnerExceptions)
                    {
                        _logger.LogError(ie, "Failed to create dns record.");
                        if (ie is DnsRecordAlreadyExistsException)
                        {
                            var exp = ie as DnsRecordAlreadyExistsException;
                            _logger.LogError(ex, $"DNS Record '{exp.commandParameters["record"]}' already exists.");
                        }
                    }
                }

            }

            var ipTask = GetPublicIpAddress();

            Task<DnsListResult> dnsRecordsListTask = null;
            try
            {
                dnsRecordsListTask = _client.DnsListRecords();
                Task.WaitAll(new Task[] {ipTask, dnsRecordsListTask});
            }
            catch (DreamHostApiException ex)
            {
                var waitTimespan = TimeSpan.FromMilliseconds(_settings.CurrentValue.CheckIntervalInMs * 10);
                var now = DateTime.Now + waitTimespan;
                _logger.LogCritical($"Dreamhost Api Exception: {ex.Message}. Next retry in {waitTimespan.TotalMinutes} minutes at {now:F}.");
                _workTimer.Change(
                    _settings.CurrentValue.CheckIntervalInMs * 10,
                    _settings.CurrentValue.CheckIntervalInMs * 10);
                return;
            }
            catch (AggregateException ex)
            {
                //This feels kind of hacky, but it works.
                if (ex.InnerException.GetType() == typeof(DreamHostApiException))
                {
                    var waitTimespan = TimeSpan.FromMilliseconds(_settings.CurrentValue.CheckIntervalInMs * 10);
                    var now = DateTime.Now + waitTimespan;
                    _logger.LogCritical($"Dreamhost Api Exception: {ex.InnerException.Message}. Next retry in {waitTimespan.TotalMinutes} minutes at {now:F}." );
                    _workTimer.Change(
                        _settings.CurrentValue.CheckIntervalInMs * 10,
                        _settings.CurrentValue.CheckIntervalInMs * 10);
                    return;
                }
            }

            var publicIp = ipTask.Result;

            if (Equals(publicIp, IPAddress.None))
            {
                _logger.LogError("Failed to read public ip address. Skipping.");
                return;
            }

            var dnsRecordsResult = dnsRecordsListTask.Result;

            if (dnsRecordsResult == null || !dnsRecordsResult.Success || !dnsRecordsResult.Data.Any())
            {
                _logger.LogError("Failed to get DNS data from api. Skipping.");
                return;
            }

            if (_settings.CurrentValue.Domains == null || !_settings.CurrentValue.Domains.Any())
            {
                _logger.LogError("Configuration is invalid Verify configuration file and try again. Skipping.");
                return;
            }

            foreach (var domain in _settings.CurrentValue.Domains)
            {
                var records = dnsRecordsResult.Data.Where(x => 
                    String.Equals(x.Record, domain.DomainName, StringComparison.CurrentCultureIgnoreCase) && 
                    x.Editable == "1" && 
                    String.Equals(x.Type, domain.Type, StringComparison.CurrentCultureIgnoreCase)
                    ).ToList();
                if (records.Any())
                {
                    var doUpdate = true;
                    foreach (var record in records)
                    {
                        if (record.Value == publicIp.ToString())
                        {
                            doUpdate = false;
                            _logger.LogDebug("Existing record matches DNS. No update needed.");
                            break; //found the same record, skipping update cycle.
                        }
                    }

                    if (doUpdate)
                    {
                        _logger.LogDebug("Starting Record update operations as public ip address does not match DNS.");
                        DeleteDnsRecords(records); //no idea which records are bad and it doesn't matter
                        CreateDnsRecords(publicIp);
                    }
                }
                else
                {
                    _logger.LogDebug("Creating Record as record does not currently exist in DNS.");
                    CreateDnsRecords(publicIp);
                }
            }

            _workTimer.Change(_settings.CurrentValue.CheckIntervalInMs * 10,_settings.CurrentValue.CheckIntervalInMs * 10);
            _logger.LogDebug("Done with loop.");
        }

        public void Start()
        {
            _workTimer.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(_settings.CurrentValue.CheckIntervalInMs));
        }

        public void Stop()
        {
            _workTimer.Change(Timeout.InfiniteTimeSpan, TimeSpan.FromMilliseconds(-1));
        }

        private async Task<IPAddress> GetPublicIpAddress()
        {
            try
            {
                var response = await _httpClient.GetAsync(RemoteScript);

                response.EnsureSuccessStatusCode();

                var xmlStr = await response.Content.ReadAsStringAsync();

                var doc = new XmlDocument();
                doc.LoadXml(xmlStr);
                var ipNode = doc.SelectSingleNode("ip/address");

                return IPAddress.Parse(ipNode.InnerText);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to get public ip address from remote system.");
            }
            return IPAddress.None;
        }
    }
}
