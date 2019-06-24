using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dreamhost.Api
{
    /// <summary>
    /// IP Address helper service
    /// </summary>
    public class IpAddressService
    {
        private readonly ILogger<IpAddressService> _logger;

        public IpAddressService(ILogger<IpAddressService> logger)
        {
            _logger = logger;
        }

        public Task<string> FetchExternalIpAddress()
        {
            _logger.LogCritical("Fake Service called.");
            return Task.FromResult("172.0.0.1");
        }
    }
}
