using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using DnsClientServiceAgent;
using Dreamhost.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DreamhostDnsApiTestApp
{
    class Program
    {
        static async Task Main(string[] args)
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
            containerBuilder.RegisterType<DreamhostApiClient>();
            containerBuilder.RegisterType<IpAddressService>();

            var container = containerBuilder.Build();

            var client = container.Resolve<DreamhostApiClient>();
            var settings = container.Resolve<IOptionsMonitor<ApplicationSettings>>();
            var loggerFactory = container.Resolve<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<Program>();
            var ipService = container.Resolve<IpAddressService>();

            logger.LogInformation($"Setting APIKEY to {settings.CurrentValue.ApiKey}");
            client.ApiKey = settings.CurrentValue.ApiKey;

            var accessCheck = await client.CheckKeyAccess(new[] {"dns-list_records"});

            var apiRecords = await client.DnsListRecords();

            if (apiRecords.Success)
            {
                Console.WriteLine($"Successfully read {apiRecords.Data.Count} DNS records from the API");
                foreach (var record in apiRecords.Data)
                {
                    if (record.Type == "A")
                        Console.WriteLine(
                        $"{record.Zone} {record.Record} {record.Type}  {record.Value}; {record.Editable} # {record.Comment}");
                }

                foreach (var recordToUpdate in settings.CurrentValue.Domains)
                {
                    var records = apiRecords.Data
                        .Where(x => x.@Type == recordToUpdate.@Type && x.Editable == "1" &&
                                    x.Zone == recordToUpdate.Zone && x.Record == recordToUpdate.DomainName);

                    foreach (var record in records)
                    {
                        var success = await client.DnsRemoveRecord(record);
                        if (success.Result != "success")
                        {
                            Console.WriteLine($"Failed to remove record: {record.Record} " +
                                              $"in zone {record.Zone} of type {record.Type} " +
                                              $"with value of {record.Value} on account {record.AccountId}");
                        }
                        else
                        {
                            Console.WriteLine($"Removed record {record.Record}={record.Value}.");
                        }
                    }
                }

                var recordSettings = settings.CurrentValue.Domains.First();
                var myIp = await ipService.FetchExternalIpAddress();
                var addSuccess = await client.DnsAddRecord(recordSettings.DomainName, myIp, recordSettings.Type, "Test Data");
                if (addSuccess.Result != "success")
                {
                    Console.WriteLine("Failed to add record.");
                }
                else
                {
                    Console.WriteLine($"Successfully added the record {recordSettings.DomainName} with value of {myIp}");
                }
            }
        }
    }
}
