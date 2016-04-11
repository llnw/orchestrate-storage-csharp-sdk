using ApiClientLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace ApiTests
{
    [TestClass]
    public class SetMTimeTests : BaseTest
    {
        [TestMethod]
        public void Test_SetMTime_Success()
        {
            var localPath = this.Fixture.CreateFile("Foo");
            var remotePath = "/" + Path.GetFileName(localPath);
            var toRemotePath = remotePath + ".new";
            var mtime = 123456;

            var result = this.Client.MakeFile(localPath, remotePath);
            this.Client.SetMTime(remotePath, mtime);

            var stat = this.Client.Stat(remotePath);
            Assert.AreEqual(mtime, stat.Mtime);
        }
    }
}
