using ApiClientLib;
using Jayrock.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace ApiTests
{
    [TestClass]
    public class RpcRetryTests : BaseTest
    {
        [TestMethod]
        public void Test_Retries_Execute_And_Can_Complete_Successfully()
        {
            var returnMe = new JsonNumber("0");

            var transport = new FakeJsonTransport();
            transport.SetReturn(this.Fixture.GetBadLoginJson());
            transport.SetReturn(this.Fixture.GetBadLoginJson());
            transport.SetReturn(this.Fixture.GetGoodLoginJson());
            transport.SetReturn(returnMe);
            
            var loginMan = new LoginManager("user", "password", transport);
            var retry = new RpcRetry(transport, loginMan, 3);
            var argMaker = new AutoTokenArgMaker("fooarg");
            var codeGetter = new NullCodeGetter();
            var result = retry.Invoke("method", argMaker, codeGetter);
            Assert.AreEqual(result, returnMe);
        }

        [TestMethod]
        public void Test_Retries_Execute_And_Should_Fail()
        {
            var expectedCode = -10001;

            try
            {
                var transport = new FakeJsonTransport();
                transport.SetReturn(this.Fixture.GetBadLoginJson());
                transport.SetReturn(this.Fixture.GetBadLoginJson());
                transport.SetReturn(this.Fixture.GetBadLoginJson());
                transport.SetReturn(this.Fixture.GetBadLoginJson());

                var loginMan = new LoginManager("user", "password", transport);
                var retry = new RpcRetry(transport, loginMan, 3);
                var argMaker = new AutoTokenArgMaker("fooarg");
                var codeGetter = new FakeCodeGetter(expectedCode);
                var result = retry.Invoke("method", argMaker, codeGetter);
                Assert.Fail("Expected ApiException");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ApiException));
                Assert.AreEqual(((ApiException)ex).AgileStatusCode, expectedCode);
            }
        }
    }


    [TestClass]
    public class HttpRetryTests : BaseTest
    {
        [TestMethod]
        public void Test_Retries_Execute_And_Can_Complete_Successfully()
        {
            var returnMe = new WebHeaderCollection();
            returnMe.Add("X-Agile-Status", "0");

            var transport = new FakeJsonTransport();
            transport.SetReturn(this.Fixture.GetBadLoginJson());
            transport.SetReturn(this.Fixture.GetBadLoginJson());
            transport.SetReturn(this.Fixture.GetGoodLoginJson());
            transport.SetReturn(returnMe);

            var loginMan = new LoginManager("user", "password", transport);
            var retry = new HttpRetry(transport, loginMan, 3);
            var codeGetter = new NullCodeGetter();
            var stream = new MemoryStream(new byte[] { 100 });
            var result = retry.Invoke("http://", stream, "", new Dictionary<string, string>());

            Assert.AreEqual(result, returnMe);
        }

        [TestMethod]
        public void Test_Retries_Execute_And_Should_Fail()
        {
            var expectedCode = 403;

            try
            {
                var transport = new FakeJsonTransport();
                transport.SetReturn(this.Fixture.GetBadLoginJson());
                transport.SetReturn(this.Fixture.GetBadLoginJson());
                transport.SetReturn(this.Fixture.GetBadLoginJson());
                transport.SetReturn(this.Fixture.GetBadLoginJson());

                var loginMan = new LoginManager("user", "password", transport);
                var retry = new HttpRetry(transport, loginMan, 3);
                var codeGetter = new FakeCodeGetter(expectedCode);
                var stream = new MemoryStream();
                var result = retry.Invoke(this.Fixture.GetApiUrl(), stream, "", null);
                Assert.Fail("Expected ApiException");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ApiException));
                Assert.AreEqual(((ApiException)ex).HttpStatusCode, expectedCode);
            }
        }
    }
}
