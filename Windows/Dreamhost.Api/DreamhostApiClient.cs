using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DnsClientServiceAgent.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Dreamhost.Api
{
    /// <summary>
    /// Dreamhost Api Client (C#)
    /// (c) 2016 Eric Hobbs eric_@_badpointer.net
    /// </summary>
    public class DreamhostApiClient
    {
        public string ApiKey { set; get; }

        public string ApiHostName { get; set; }

        private readonly ILogger<DreamhostApiClient> _logger;

        private HttpClient HttpClient { get; set; }

        /// <summary>
        /// Constructs a new instance of the API
        /// </summary>
        public DreamhostApiClient(ILogger<DreamhostApiClient> logger)
        {
            _logger = logger;
            ApiHostName = "https://api.dreamhost.com";

            HttpClient = new HttpClient {BaseAddress = new Uri(ApiHostName)};
        }


        /// <summary>
        /// List All DNS Records
        /// </summary>
        /// <returns></returns>
        public async Task<DnsListResult> DnsListRecords()
        {
            var response = await GetApiResult("dns-list_records", null);
            return new DnsListResult
            {
                Result = "error",
                Records = null,
            };
        }

        /// <summary>
        /// Adds a DNS Record
        /// </summary>
        /// <param name="record">name of the record.</param>
        /// <param name="value">Value of the record.</param>
        /// <param name="type">Record type (A,MX,...)</param>
        /// <param name="comment">Optional comment.</param>
        /// <returns>true if the record was added successfully.</returns>
        public async Task<bool> DnsAddRecord(string record, string value, string type, string comment)
        {
            var response = await GetApiResult("dns-add_record", null);
            return false;
        }

        /// <summary>
        /// Removes a DNS record.
        /// </summary>
        /// <param name="record"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns>true if the record was deleted.</returns>
        public async Task<bool> DnsRemoveRecord(string record, string type, string value)
        {
            var response = await GetApiResult("dns-remove_record", null);
            return false;
        }

        /// <summary>
        /// Checks if the key has access to the requested methods
        /// </summary>
        /// <param name="commands">api commands to check that the key has access rights for.</param>
        /// <returns>true if the key has access to all of the specified keys.</returns>
        public async Task<bool> CheckKeyAccess(IList<string> commands)
        {
            var response = await GetApiResult("api-list_accessible_cmds", null);

            var missingCommands = new List<string>();
            missingCommands.AddRange(commands);

            var obj = JsonConvert.DeserializeObject<ApiResult>(response);

            if (obj.Result != "success")
            {
                _logger.LogError("api-list_accessible_cmds failed: " + obj.Reason);
                return false;
            }

            try
            {
                foreach (var cmds in obj.Data)
                {
                    var apicmd = cmds.cmd.ToString();
                    if (missingCommands.Contains(apicmd))
                        missingCommands.Remove(apicmd);
                }
                return missingCommands.Count == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error during CheckKeyAccess serialization.", ex);
            }

            return false;
        }

        /// <summary>
        /// Generate UUID for each apicall.
        /// Technically we can reuse the last id if the last call failed
        /// However, we would need to store that information - UUID generation is cheap.
        /// </summary>
        /// <returns>GUID formatted via digits formatter.</returns>
        private static string GenerateUuid()
        {
            return Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// Build the uri request string.
        /// </summary>
        /// <param name="command">api command</param>
        /// <param name="parameters">api command parameters</param>
        /// <returns>Uri</returns>
        private Uri BuildUri(string command, IDictionary<string, string> parameters)
        {
            return new 
                Uri($"{ApiHostName}/" +
                    $"?key={ApiKey}" +
                    $"&cmd={command}" +
                    $"&uuid={GenerateUuid()}" +
                    $"&format=json" +
                    $"{parameters.ToQueryString()}");
        }

        /// <summary>
        /// Call the specified api endpoint and get the result value.
        /// </summary>
        /// <param name="command">api command</param>
        /// <param name="additionalParameters">command parameters dictionary. Can be null.</param>
        /// <returns></returns>
        private async Task<string> GetApiResult(string command, IDictionary<string, string> additionalParameters)
        {
            HttpResponseMessage response = await HttpClient.GetAsync(BuildUri(command, additionalParameters));

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            _logger.LogError("Failed API call: " + response.ReasonPhrase);
            return null;
        }
    }
}
