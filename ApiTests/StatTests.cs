using ApiClientLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace ApiTests
{
    [TestClass]
    public class StatTests : BaseTest
    {
        [TestMethod]
        public void Test_Stat_Success()
        {
            var localPath = this.Fixture.CreateFile("Foo");
            var remotePath = "/" + Path.GetFileName(localPath);

            var result = this.Client.MakeFile(localPath, remotePath);
            Assert.AreEqual(0, result.Status);

            var statResult = this.Client.Stat(remotePath);

            Assert.AreEqual(0, statResult.Code);
            Assert.IsTrue(statResult.Ctime > 0);
            Assert.IsTrue(statResult.Gid > 0);
            Assert.IsTrue(statResult.Uid > 0);
            Assert.AreEqual(this.Fixture.GetDefaultFileSize(), statResult.Size);
            Assert.AreEqual(2, statResult.Type);
            Assert.AreEqual(this.Fixture.GetDefaultChecksum(), statResult.Checksum);
        }

        [TestMethod]
        public void Test_Stat_Object_Not_Found()
        {
            try
            {
                var remotePath = "/fake/file/here";
                var statResult = this.Client.Stat(remotePath);
                Assert.Fail("Expected ApiException");
            }
            catch (ApiException ex)
            {
                Assert.AreEqual(-1, ex.AgileStatusCode);
            }
        }
    }
}
