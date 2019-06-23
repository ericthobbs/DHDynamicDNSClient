using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DreamhostApi.Test
{
    [TestClass]
    public class ClientTests
    {
        [TestMethod]
        public async Task Apikey_ExampleKey_Invalid_Test()
        {
            var client = new Dreamhost.Api.DreamhostApiClient("TESTBADKEY1");

            var result = await client.CheckKeyAccess(new[] { "user-list_users" });

            Assert.IsFalse(result, "example key is valid.");
        }

        [TestMethod]
        public async Task CheckAccess_ExampleKey_Valid_Test()
        {
            var client = new Dreamhost.Api.DreamhostApiClient("6SHU5P2HLDAYECUM");

            var result = await client.CheckKeyAccess(new [] { "user-list_users_no_pw" });

            Assert.IsTrue(result, "expected command 'user-list_users_no_pw' not available.");
        }

        [TestMethod]
        public async Task CheckAccess_ExampleKey_Invalid_Test()
        {
            var client = new Dreamhost.Api.DreamhostApiClient("6SHU5P2HLDAYECUM");

            var result = await client.CheckKeyAccess(new[] { "account-list_keys" });

            Assert.IsFalse(result, "check key access failed. demo key does not have access to account-list_keys");
        }
    }
}
