using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnsClientServiceAgent.Extensions
{
    public static class DictionaryExtensions
    {
        public static string ToQueryString(this IDictionary<string, string> collection)
        {
            if (collection == null) return string.Empty;

            var sb = new StringBuilder();
            foreach (var p in collection.Keys)
            {
                sb.Append($"&{p}={collection[p]}");
            }
            return sb.ToString();
        }
    }
}
