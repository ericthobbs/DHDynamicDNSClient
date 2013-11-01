using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamicDNSAgentService
{
	class DNSEntryNotFound: Exception
	{
		public DNSEntryNotFound(String str) : base(str) { }
	}
}
