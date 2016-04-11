using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace ApiTests
{
    [TestClass]
    public class MakeFileTests : BaseTest
    {
        [TestMethod]
        public void Test_MakeFile_Success()
        {
            var localPath = this.Fixture.CreateFile("Foo");
            var remotePath = "/" + Path.GetFileName(localPath);

            var result = this.Client.MakeFile(localPath, remotePath);
            Assert.IsTrue(result.Path.Contains(Path.GetFileName(localPath)));
            Assert.AreEqual(3, result.Size);
            Assert.AreEqual(0, result.Status);
            Assert.AreEqual(this.Fixture.GetDefaultChecksum(), result.Checksum);
        }

        [TestMethod]
        public void Test_MakeFile_Success_With_Extra_Headers()
        {
            var localPath = this.Fixture.CreateFile("Foo");
            var remotePath = "/" + Path.GetFileName(localPath);
            var headers = new Dictionary<string, string>() {
                { "X-Agile-Expose-Egress", "COMPLETE" },
                { "X-Agile-MTime", "123456" },
            };

            var result = this.Client.MakeFile(localPath, remotePath, headers);
            Assert.AreEqual(0, result.Status);
        }
    }
}
