using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using log4net;

namespace DnsClientServiceAgent
{
    /// <summary>
    /// Dreamhost Api Client (C#)
    /// </summary>
    class DreamhostApiClient
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DreamhostApiClient));
        private readonly string _apiServer;
        private readonly string _apikey;

        public DreamhostApiClient(string apiserver, string apikey)
        {
            if (string.IsNullOrEmpty(apikey))
                throw new ArgumentNullException(nameof(apikey));

            _apikey = apikey;

            if (string.IsNullOrEmpty(apiserver))
                throw new ArgumentNullException(nameof(apiserver));

            _apiServer = apiserver;
        }

        public async Task<DnsListResult> DnsListRecords()
        {
            var response = await GetApiResult("dns-list_records", null);
            return new DnsListResult
            {
                Result = "error",
                Records = null,
            };
        }

        public async Task<ApiResult> DnsAddRecord(string record, string value, string type, string comment)
        {
            var response = await GetApiResult("dns-add_record", null);
            return new ApiResult
            {
                Result = "error"
            };
        }

        public async Task<ApiResult> DnsRemoveRecord(string record, string type, string value)
        {
            var response = await GetApiResult("dns-remove_record", null);
            return new ApiResult
            {
                Result = "error"
            };
        }

        /// <summary>
        /// Checks if the key has access to the requested methods
        /// </summary>
        /// <param name="commands"></param>
        /// <returns></returns>
        public async Task<bool> CheckKeyAccess(string[] commands)
        {
            var response = await GetApiResult("api-list_accessible_cmds", null);
            return false;
        }

        private static string GenerateUuid()
        {
            return Guid.NewGuid().ToString("N");
        }

        private Task<string> GetApiResult(string command, IDictionary<string, string> additionalParameters)
        {
            var client = new HttpClient();
            return client.GetStringAsync(new Uri(_apiServer));
        }
    }
}
