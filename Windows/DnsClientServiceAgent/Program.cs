using System;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Dreamhost.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Topshelf;
using Topshelf.Autofac;

namespace DnsClientServiceAgent
{
    /// <summary>
    /// Service Application Entry Point
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddUserSecrets<Program>();

            var configuration = builder.Build();


            var serviceCollection = new ServiceCollection();
            var containerBuilder = new ContainerBuilder();

            serviceCollection.AddLogging();
            serviceCollection.AddAutofac();

            serviceCollection.Configure<ApplicationSettings>(configuration.GetSection("Settings"));

            //Register Types
            containerBuilder.Populate(serviceCollection);
            containerBuilder.RegisterType<DynamicDnsUpdateService>();
            containerBuilder.RegisterType<DreamhostApiClient>();
            containerBuilder.RegisterType<IpAddressService>();

            var container = containerBuilder.Build();

            HostFactory.Run(x =>
            {
                x.UseAutofacContainer(container);

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
    }
}
