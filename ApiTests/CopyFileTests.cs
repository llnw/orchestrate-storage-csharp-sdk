using ApiClientLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace ApiTests
{
    [TestClass]
    public class CopyFileTests : BaseTest
    {
        [TestMethod]
        public void Test_CopyFile_Success()
        {
            var localPath = this.Fixture.CreateFile("Foo");
            var remotePath = "/" + Path.GetFileName(localPath);
            var toRemotePath = remotePath + ".new";
            
            var result = this.Client.MakeFile(localPath, remotePath);
            this.Client.CopyFile(remotePath, toRemotePath);
        }

        [TestMethod]
        public void Test_CopyFile_Source_Does_Not_Exist()
        {
            var fromPath = "/NotARealFile.txt";
            var toPath = "/AlsoNotReal.txt";
            try
            {
                this.Client.CopyFile(fromPath, toPath);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ApiException));
                Assert.AreEqual(-1, ((ApiException)ex).AgileStatusCode);
            }
        }

        [TestMethod]
        public void Test_CopyFile_Destination_Does_Not_Exist()
        {
            var localPath = this.Fixture.CreateFile("Foo");
            var remotePath = "/" + Path.GetFileName(localPath);
            this.Client.MakeFile(localPath, remotePath);
            var toPath = "/Also/NotReal.txt";
            
            try
            {
                this.Client.CopyFile(remotePath, toPath);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ApiException));
                Assert.AreEqual(-2, ((ApiException)ex).AgileStatusCode);
            }
        }
    }
}
