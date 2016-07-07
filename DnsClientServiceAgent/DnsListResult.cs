using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnsClientServiceAgent
{
    class DnsListResult : ApiResult
    {
        public IList<DnsRecord> Records { get; set; }
    }
}
