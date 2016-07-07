using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DreamhostApi.Test
{
    [TestClass]
    public class ClientTests
    {
        [TestMethod]
        public void Apikey_ExampleKey_Invalid_Test()
        {
            var client = new Dreamhost.Api.DreamhostApiClient("https://api.dreamhost.com", "TESTBADKEY1");

            var result = client.CheckKeyAccess(new[] { "user-list_users" });
            result.Wait();

            Assert.IsFalse(result.Result, "example key is valid.");
        }

        [TestMethod]
        public void CheckAccess_ExampleKey_Valid_Test()
        {
            var client = new Dreamhost.Api.DreamhostApiClient("https://api.dreamhost.com", "6SHU5P2HLDAYECUM");

            var result = client.CheckKeyAccess(new [] { "user-list_users_no_pw" });
            result.Wait();

            Assert.IsTrue(result.Result, "expected command 'user-list_users_no_pw' not available.");
        }

        [TestMethod]
        public void CheckAccess_ExampleKey_Invalid_Test()
        {
            var client = new Dreamhost.Api.DreamhostApiClient("https://api.dreamhost.com", "6SHU5P2HLDAYECUM");

            var result = client.CheckKeyAccess(new[] { "user-list_users" });
            result.Wait();

            Assert.IsFalse(result.Result, "example key is valid.");
        }
    }
}
