using ApiClientLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace ApiTests
{
    [TestClass]
    public class BaseTest
    {
        public TestFixture Fixture;
        public ApiClient Client;

        [TestInitialize]
        public void Init()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            this.Fixture = new TestFixture();
            this.Client = this.Fixture.GetClient();
        }
    }
}
