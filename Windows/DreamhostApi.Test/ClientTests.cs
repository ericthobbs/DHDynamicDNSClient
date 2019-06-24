using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DreamhostApi.Test
{
    [TestClass]
    public class ClientTests
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            context.WriteLine("Settings Configured");
            context.WriteLine("Logging Configured");
        }

        [TestMethod]
        public async Task ApiKey_ExampleKey_Invalid_Test()
        {
            var client = new Dreamhost.Api.DreamhostApiClient(null);
            client.ApiKey = "TESTBADKEY1";
            var result = await client.CheckKeyAccess(new[] { "user-list_users" });

            Assert.IsFalse(result.Item1, "example key is valid.");
        }

        [TestMethod]
        public async Task CheckAccess_ExampleKey_Valid_Test()
        {
            var client = new Dreamhost.Api.DreamhostApiClient(null);
            client.ApiKey = "6SHU5P2HLDAYECUM";
            var result = await client.CheckKeyAccess(new [] { "user-list_users_no_pw" });

            Assert.IsTrue(result.Item1, "expected command 'user-list_users_no_pw' not available.");
        }

        [TestMethod]
        public async Task CheckAccess_ExampleKey_Invalid_Test()
        {
            var client = new Dreamhost.Api.DreamhostApiClient(null);
            client.ApiKey = "6SHU5P2HLDAYECUM";
            var result = await client.CheckKeyAccess(new[] { "account-list_keys" });

            Assert.IsFalse(result.Item1, "check key access failed. demo key does not have access to account-list_keys");
        }
    }
}
