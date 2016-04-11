using ApiClientLib;
using Jayrock.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ApiTests
{
    [TestClass]
    public class LoginTests : BaseTest
    {
        [TestMethod]
        public void Test_Login_Raises_Authentication_Exception()
        {
            var client = this.Fixture.GetClient("fake", "bad");
            try
            {
                var result = client.Login();
                Assert.Fail("Failed to throw API Exception");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(LoginException));
            }
        }

        [TestMethod]
        public void Test_Login_Success()
        {
            var result = this.Client.Login();
            Assert.IsTrue(result.Gid > 0);
            Assert.IsTrue(result.Uid > 0);
            Assert.IsNotNull(result.Token);
            Assert.AreEqual(36, result.Token.Length); // "b36c60c6-063d-456f-8bc5-1fe9127148d8"
            Assert.IsNotNull(result.Path);
        }

        [TestMethod]
        public void Test_Login_Tokens_Are_Reset_If_Called_Directly() 
        {
            var result = this.Client.Login();
            var oldToken = result.Token;
            var result2 = this.Client.Login();
        
            Assert.AreNotEqual(oldToken, result2.Token);
        }

        [TestMethod]
        public void Test_Login_Tokens_Are_Reused()
        {
            var result = this.Client.Login();
            var oldToken = result.Token;
            this.Client.ListFile("/", 10, 0, true);
            
            Assert.AreEqual(oldToken, this.Client.GetToken());
        }

        [TestMethod]
        public void Test_Login_Tokens_Are_Reset_If_Expired()
        {
            var transport = new FakeJsonTransport();
            transport.SetReturn(this.Fixture.GetGoodLoginJson("oldtoken"));
            transport.SetReturn(new JsonNumber("-10001"));
            transport.SetReturn(this.Fixture.GetGoodLoginJson("newtoken"));
            transport.SetReturn(new JsonNumber("0"));

            var loginMan = new LoginManager(this.Fixture.GetUser(), this.Fixture.GetPassword(), transport);
            var rpcRetry = new RpcRetry(transport, loginMan);
            var httpRetry = new HttpRetry(transport, loginMan);
            var client = new ApiClient(this.Fixture.GetApiUrl(), rpcRetry, httpRetry, loginMan);
            var result = client.Login();
            var oldToken = result.Token;
         
            client.DeleteFile("/notrealatall.txt");

            Assert.AreNotEqual(oldToken, this.Client.GetToken());
        }
    }
}
