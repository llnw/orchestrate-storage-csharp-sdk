using ApiClientLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace ApiTests
{
    [TestClass]
    public class ListFileTests : BaseTest
    {
        [TestMethod]
        public void Test_ListFile_Success()
        {
            var localPath = this.Fixture.CreateFile("foo");
            var localFileName = Path.GetFileName(localPath); ;
            var remotePath = "/" + localFileName;
            this.Client.MakeFile(localPath, remotePath);
            var listResults = this.Client.ListFile("/", 100, 0, true);
            ListFileResult foundResult = null;
            foreach (var file in listResults)
            {
                if (file.Name == localFileName)
                {
                    foundResult = file;
                }
            }
            Assert.AreEqual(localFileName, foundResult.Name);
            Assert.IsTrue(foundResult.Ctime > 0);
            Assert.IsTrue(foundResult.Mtime > 0);
            Assert.IsTrue(foundResult.Gid > 0);
            Assert.IsTrue(foundResult.Uid > 0);
            Assert.AreEqual(3, foundResult.Size);
            Assert.IsNotNull(foundResult.Checksum);
            this.Client.DeleteFile(remotePath);
        }

        [TestMethod]
        public void Test_ListFile_Without_Stat()
        {

            var localPath = this.Fixture.CreateFile("foo");
            var localFileName = Path.GetFileName(localPath);
            var remotePath = "/" + localFileName;
            this.Client.MakeFile(localPath, remotePath);
            var listResults = this.Client.ListFile("/", 100, 0, false);
            ListFileResult foundResult = null;
            foreach (var file in listResults)
            {
                if (file.Name == localFileName)
                {
                    foundResult = file;
                }
            }
            Assert.AreEqual(localFileName, foundResult.Name);
            Assert.AreEqual(0, foundResult.Ctime);
            Assert.AreEqual(0, foundResult.Mtime);
            Assert.AreEqual(0, foundResult.Gid);
            Assert.AreEqual(0, foundResult.Uid);
            Assert.IsNull(foundResult.Checksum);
            Assert.AreEqual(0, foundResult.Size);
            Assert.IsNull(foundResult.ContentType);
            this.Client.DeleteFile(remotePath);
        }

        [TestMethod]
        public void Test_ListFile_Cookie_Paging()
        {
            var names = new List<string>();
            for (int i = 0; i < 5; i++)
            {
                var localPath = this.Fixture.CreateFile("foo");
                var localFileName = Path.GetFileName(localPath);
                var remotePath = "/" + localFileName;
                names.Add(remotePath);
                this.Client.MakeFile(localPath, remotePath);
            }
            
            var listResults = this.Client.ListFile("/", 3, 0, true);
            Assert.AreEqual(3, listResults.Count);
            Assert.IsTrue(listResults.Cookie > 0);

            listResults = this.Client.ListFile("/", 1000, listResults.Cookie, true);
            Assert.IsTrue(listResults.Count > 0);
            Assert.AreEqual(0, listResults.Cookie);

            foreach (string name in names)
            {
                this.Client.DeleteFile(name);
            }
        }
    }
}
