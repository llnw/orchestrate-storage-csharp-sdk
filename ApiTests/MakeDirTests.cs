using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiTests
{
    [TestClass]
    public class MakeDirTests : BaseTest
    {
        [TestMethod]
        public void Test_MakeDir_Success()
        {
            var remotePath = "/fakedir";
            this.Client.DeleteDir(remotePath);
            this.Client.MakeDir(remotePath);
        }
    }
}
