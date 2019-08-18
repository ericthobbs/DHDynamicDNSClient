using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace DreamhostApi.Test
{
    [TestClass]
    public class ClientTests
    {
        private static IConfigurationRoot _configuration { get; set; }
        private static IServiceProvider _serviceProvider { get; set; }

        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            _configuration = new ConfigurationBuilder().Build();
            context.WriteLine("Settings Configured");

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging(cfg =>
            {
                cfg.AddConfiguration(_configuration.GetSection("Logging"));
                cfg.AddDebug();
                cfg.AddConsole();
            });
            context.WriteLine("Logging Configured");

            serviceCollection.AddHttpClient<Dreamhost.Api.DreamhostApiClient>();
            serviceCollection.AddScoped<Dreamhost.Api.DreamhostApiClient>();
            context.WriteLine("Types Registered");

            _serviceProvider = serviceCollection.BuildServiceProvider();
            context.WriteLine("DI Configured");
        }

        [TestMethod]
        [DataRow("TESTBADKEY1")]
        [ExpectedException(typeof(Dreamhost.Api.Exceptions.DreamHostApiException))]
        public async Task ApiKey_ExampleKey_Invalid_Test(string apikey)
        {
            var client = _serviceProvider.GetRequiredService<Dreamhost.Api.DreamhostApiClient>();
            client.ApiKey = apikey;
            var result = await client.CheckKeyAccess(new[] { "user-list_users" });

            Assert.IsFalse(result.Item1, "example key is valid.");
        }

        [TestMethod]
        [DataRow("6SHU5P2HLDAYECUM")]
        public async Task CheckAccess_ExampleKey_Valid_Test(string apikey)
        {
            var client = _serviceProvider.GetRequiredService<Dreamhost.Api.DreamhostApiClient>();
            client.ApiKey = apikey;
            var result = await client.CheckKeyAccess(new [] { "user-list_users_no_pw" });

            Assert.IsTrue(result.Item1, "expected command 'user-list_users_no_pw' not available.");
        }

        [TestMethod]
        [DataRow("6SHU5P2HLDAYECUM")]
        //[ExpectedException(typeof(Dreamhost.Api.Exceptions.DreamHostApiException))]
        public async Task CheckAccess_ExampleKey_Invalid_Test(string apikey)
        {
            var client = _serviceProvider.GetRequiredService<Dreamhost.Api.DreamhostApiClient>();
            client.ApiKey = apikey;
            var result = await client.CheckKeyAccess(new[] { "account-list_keys" });
            Assert.IsFalse(result.Item1, "check key access failed. demo key does not have access to account-list_keys");
        }
    }
}
