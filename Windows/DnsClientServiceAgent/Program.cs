using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace DnsClientServiceAgent
{
    class Program
    {
        static void Main(string[] args)
        {

            HostFactory.Run(x =>
            {
                x.UseLog4Net();
                x.Service<DnsManagerService>(s =>
                {
                    s.ConstructUsing(name => new DnsManagerService() );
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalService();

                x.SetDescription("Configures remote dns service with this machines public ip.");
                x.SetDisplayName("DnsClientServiceAgent");
                x.SetServiceName("DnsClientServiceAgent");
            });
    }
    }
}
