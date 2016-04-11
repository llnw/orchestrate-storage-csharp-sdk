using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiTests
{
    [TestClass]
    public class DeleteDirTests : BaseTest
    {
        [TestMethod]
        public void Test_DeleteDir_Success()
        {
            var remotePath = "/fakedir";
            this.Client.DeleteDir(remotePath);
            this.Client.MakeDir(remotePath);
            this.Client.DeleteDir(remotePath);
        }

        [TestMethod]
        public void Test_DeleteDir_Success_If_Doesnt_Exist()
        {
            var remotePath = "/fakedir";
            this.Client.DeleteDir(remotePath);
        }
    }
}
