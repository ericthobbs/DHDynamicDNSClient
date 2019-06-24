using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Dreamhost.Api;
using Dreamhost.Api.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Topshelf;
using Topshelf.Autofac;

namespace DnsClientServiceAgent
{
    class Program
    {
        static void Main(string[] args)
        {

            IConfigurationRoot Configuration;
            var builder = new ConfigurationBuilder()
                .SetBasePath(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddUserSecrets<Program>();

            Configuration = builder.Build();


            var serviceCollection = new ServiceCollection();
            var containerBuilder = new ContainerBuilder();

            serviceCollection.AddLogging();
            serviceCollection.AddAutofac();

            serviceCollection.Configure<ApplicationSettings>(Configuration.GetSection("Settings"));

            //Register Types
            containerBuilder.Populate(serviceCollection);
            containerBuilder.RegisterType<DynamicDnsUpdateService>();
            containerBuilder.RegisterType<DreamhostApiClient>();

            var container = containerBuilder.Build();

            try
            {
                HostFactory.Run(x =>
                {
                    var y = x.UseAutofacContainer(container);

                    x.Service<DynamicDnsUpdateService>(s =>
                    {
                        s.ConstructUsingAutofacContainer();
                        s.WhenStarted(tc => tc.Start());
                        s.WhenStopped(tc => tc.Stop());
                    });
                    x.RunAsLocalService();

                    x.SetDescription("Configures remote DNS update service with this machines public IP address.");
                    x.SetDisplayName("DnsClientServiceAgent");
                    x.SetServiceName("DnsClientServiceAgent");
                });
            }
            catch (Exception ex)
            {
                int a = 0;
            }
        }
    }
}
