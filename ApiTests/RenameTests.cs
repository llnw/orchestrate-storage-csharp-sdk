using ApiClientLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace ApiTests
{
    [TestClass]
    public class RenameTests : BaseTest
    {
        [TestMethod]
        public void Test_Rename_Success()
        {
            var localPath = this.Fixture.CreateFile("Foo");
            var remotePath = "/" + Path.GetFileName(localPath);
            var toRemotePath = remotePath + ".new";
            
            var result = this.Client.MakeFile(localPath, remotePath);
            this.Client.Rename(remotePath, toRemotePath);
        }

        [TestMethod]
        public void Test_Rename_Source_Does_Not_Exist()
        {
            var fromPath = "/NotARealFile.txt";
            var toPath = "/AlsoNotReal.txt";
            try
            {
                this.Client.Rename(fromPath, toPath);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ApiException));
                Assert.AreEqual(-1, ((ApiException)ex).AgileStatusCode);
            }
        }

        [TestMethod]
        public void Test_Rename_Destination_Already_Exists()
        {
            var localPath = this.Fixture.CreateFile("Foo");
            var remotePath = "/" + Path.GetFileName(localPath);
            var toPath = "/NotReal.txt";
            this.Client.MakeFile(localPath, remotePath);
            this.Client.MakeFile(localPath, toPath);

            try
            {
                this.Client.Rename(remotePath, toPath);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ApiException));
                Assert.AreEqual(-3, ((ApiException)ex).AgileStatusCode);
            }
        }

        [TestMethod]
        public void Test_Rename_Destination_Parent_Directory_Doesnt_Exist()
        {
            var localPath = this.Fixture.CreateFile("Foo");
            var remotePath = "/" + Path.GetFileName(localPath);
            this.Client.MakeFile(localPath, remotePath);
            var toPath = "/Also/NotReal.txt";
            
            try
            {
                this.Client.Rename(remotePath, toPath);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ApiException));
                Assert.AreEqual(-3, ((ApiException)ex).AgileStatusCode);
            }
        }
    }
}
