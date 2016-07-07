using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnsClientServiceAgent
{
    public struct DnsRecord
    {
        /// <summary>
        /// Numeric ID of the account 
        /// </summary>
        public String AccountId { get; set; }

        /// <summary>
        /// User Comment
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Boolean
        /// </summary>
        public string Editable { get; set; }

        /// <summary>
        /// The domain name
        /// </summary>
        public string Record { get; set; }

        /// <summary>
        /// Type of record: A = ipv4, AAAA=ipv6, MX=mail, TXT=TEXT, etc
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The value of the record (for A/AAAA this is the ip address)
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// DNS Zone
        /// </summary>
        public string Zone { get; set; }
    };
}
