using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiTests
{
    [TestClass]
    public class DeleteObjectTests : BaseTest
    {
        [TestMethod]
        public void Test_DeleteObject_Success()
        {
            var remotePath = "/fakedir";
            this.Client.DeleteObject(remotePath);
            this.Client.MakeDir(remotePath);
            this.Client.DeleteObject(remotePath);
        }

        [TestMethod]
        public void Test_DeleteObject_Success_If_Doesnt_Exist()
        {
            var remotePath = "/fakedir";
            this.Client.DeleteObject(remotePath);
        }
    }
}
