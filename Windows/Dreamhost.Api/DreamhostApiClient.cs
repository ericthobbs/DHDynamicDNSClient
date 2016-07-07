using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DnsClientServiceAgent.Extensions;
using log4net;
using Newtonsoft.Json;

namespace Dreamhost.Api
{
    /// <summary>
    /// Dreamhost Api Client (C#)
    /// </summary>
    public class DreamhostApiClient
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DreamhostApiClient));

        private readonly string apiServer;
        private readonly string apikey;

        private const string Format = "json";

        public DreamhostApiClient(string apiserver, string apikey)
        {
            if (string.IsNullOrEmpty(apikey))
                throw new ArgumentNullException(nameof(apikey));

            this.apikey = apikey;

            if (string.IsNullOrEmpty(apiserver))
                throw new ArgumentNullException(nameof(apiserver));

            apiServer = apiserver;
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

            var obj = JsonConvert.DeserializeObject<ApiResult>(response);

            if (obj.Result == "success")
            {
                bool exists = true;
                foreach (var cmds in obj.Data)
                {
                    var d = new AvailableCommandData
                    {
                        cmd = cmds.cmd,
                        args = cmds.args,
                        optargs = cmds.optargs,
                        order = cmds.order
                    };
                    if (!commands.Contains(d.cmd))
                        exists = false;
                }
                return exists;
            }

            return false;
        }

        private static string GenerateUuid()
        {
            return Guid.NewGuid().ToString("N");
        }

        private Uri BuildUri(string command, IDictionary<string, string> parameters)
        {
            var uri = new Uri(
                $"{apiServer}?key={apikey}&cmd={command}&uuid={GenerateUuid()}&format={Format}{parameters.ToQueryString()}");

            Logger.Debug("Url built: " + uri);

            return uri;
        }

        private Task<string> GetApiResult(string command, IDictionary<string, string> additionalParameters)
        {
            var client = new HttpClient();
            return client.GetStringAsync(BuildUri(command, additionalParameters));
        }
    }
}
