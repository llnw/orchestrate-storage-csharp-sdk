using ApiClientLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace ApiTests
{
    [TestClass]
    public class ListPathTests : BaseTest
    {
        [TestMethod]
        public void Test_ListPath_Success()
        {
            var localPath = this.Fixture.CreateFile("foo");
            var localFileName = Path.GetFileName(localPath); ;
            var remotePath = "/" + localFileName;
            this.Client.MakeFile(localPath, remotePath);
            var listResults = this.Client.ListPath("/", 100, null, true);
            ListPathFileResult foundResult = null;
            foreach (var file in listResults.Files)
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
        public void Test_ListPath_Without_Stat()
        {

            var localPath = this.Fixture.CreateFile("foo");
            var localFileName = Path.GetFileName(localPath);
            var remotePath = "/" + localFileName;
            this.Client.MakeFile(localPath, remotePath);
            var listResults = this.Client.ListPath("/", 100, null, false);
            ListPathFileResult foundResult = null;
            foreach (var file in listResults.Files)
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
            Assert.AreEqual(0, foundResult.ContentType);
            this.Client.DeleteFile(remotePath);
        }

        [TestMethod]
        public void Test_ListPath_Cookie_Paging()
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
            
            var listResults = this.Client.ListPath("/", 3, null, true);
            Assert.AreEqual(3, listResults.Files.Count);
            Assert.AreNotEqual(String.Empty, listResults.Cookie);
            Assert.AreNotEqual(null, listResults.Cookie);

            listResults = this.Client.ListPath("/", 1000, listResults.Cookie, true);
            Assert.IsTrue(listResults.Files.Count > 0);
            Assert.AreEqual(null, listResults.Cookie);

            foreach (string name in names)
            {
                this.Client.DeleteFile(name);
            }
        }
    }
}
