using Newtonsoft.Json;

namespace Dreamhost.Api
{
    public class DnsRecord
    {
        /// <summary>
        /// Numeric ID of the account 
        /// </summary>
        [JsonProperty(PropertyName = "account_id")]
        public string AccountId { get; set; }

        /// <summary>
        /// User Comment
        /// </summary>
        [JsonProperty(PropertyName = "comment")]
        public string Comment { get; set; }

        /// <summary>
        /// Boolean
        /// </summary>
        [JsonProperty(PropertyName = "editable")]
        public string Editable { get; set; }

        /// <summary>
        /// The domain name
        /// </summary>
        [JsonProperty(PropertyName = "record")]
        public string Record { get; set; }

        /// <summary>
        /// Type of record: A = ipv4, AAAA=ipv6, MX=mail, TXT=TEXT, etc
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public string @Type { get; set; }

        /// <summary>
        /// The value of the record (for A/AAAA this is the ip address)
        /// </summary>
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        /// <summary>
        /// DNS Zone
        /// </summary>
        [JsonProperty(PropertyName = "zone")]
        public string Zone { get; set; }
    };
}
