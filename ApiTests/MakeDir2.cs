using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiTests
{
    [TestClass]
    public class MakeDir2Tests : BaseTest
    {
        [TestMethod]
        public void Test_MakeDir2_Success()
        {
            var remotePath = "/fakedir/in/here";
            this.Client.DeleteDir(remotePath);
            this.Client.MakeDir2(remotePath);
            this.Client.DeleteObject(remotePath);
            this.Client.DeleteObject("/fakedir/in");
            this.Client.DeleteObject("/fakedir");
        }
    }
}
