using ApiClientLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ApiTests
{
    [TestClass]
    public class LogoutTests : BaseTest
    {
        [TestMethod]
        public void Test_Logout_Success()
        {
            var loginResult = this.Client.Login();
            this.Client.Logout(loginResult.Token);
        }

        [TestMethod]
        public void Test_Logout_Using_Raises_Invalid_Token_Exception()
        {
            try
            {
                var loginResult = this.Client.Login();
                this.Client.Logout("Fake Token");
                Assert.Fail("Failed to throw API Exception");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ApiException));
            }
        }
    }
}
