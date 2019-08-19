using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace DreamhostApi.Test
{
    [TestClass]
    public class ClientTests
    {
        private static IConfigurationRoot Configuration { get; set; }
        private static IServiceProvider ServiceProvider { get; set; }

        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            Configuration = new ConfigurationBuilder().Build();
            context.WriteLine("Settings Configured");

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging(cfg =>
            {
                cfg.AddConfiguration(Configuration.GetSection("Logging"));
                cfg.AddDebug();
                cfg.AddConsole();
            });
            context.WriteLine("Logging Configured");

            serviceCollection.AddHttpClient<Dreamhost.Api.DreamhostApiClient>();
            serviceCollection.AddScoped<Dreamhost.Api.DreamhostApiClient>();
            context.WriteLine("Types Registered");

            ServiceProvider = serviceCollection.BuildServiceProvider();
            context.WriteLine("DI Configured");
        }

        [TestMethod]
        [DataRow("TESTBADKEY1")]
        [ExpectedException(typeof(Dreamhost.Api.Exceptions.DreamHostApiException))]
        public async Task ApiKey_ExampleKey_Invalid_Test(string apikey)
        {
            var client = ServiceProvider.GetRequiredService<Dreamhost.Api.DreamhostApiClient>();
            client.ApiKey = apikey;
            var result = await client.CheckKeyAccess(new[] { "user-list_users" });

            Assert.IsFalse(result.Item1, "example key is valid.");
        }

        [TestMethod]
        [DataRow("6SHU5P2HLDAYECUM")]
        public async Task CheckAccess_ExampleKey_Valid_Test(string apikey)
        {
            var client = ServiceProvider.GetRequiredService<Dreamhost.Api.DreamhostApiClient>();
            client.ApiKey = apikey;
            var result = await client.CheckKeyAccess(new [] { "user-list_users_no_pw" });

            Assert.IsTrue(result.Item1, "expected command 'user-list_users_no_pw' not available.");
        }

        [TestMethod]
        [DataRow("6SHU5P2HLDAYECUM")]
        public async Task CheckAccess_ExampleKey_Invalid_Test(string apikey)
        {
            var client = ServiceProvider.GetRequiredService<Dreamhost.Api.DreamhostApiClient>();
            client.ApiKey = apikey;
            var result = await client.CheckKeyAccess(new[] { "account-list_keys" });
            Assert.IsFalse(result.Item1, "check key access failed. demo key does not have access to account-list_keys");
        }
    }
}
