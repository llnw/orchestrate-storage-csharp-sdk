using ApiClientLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiTests
{
    [TestClass]
    public class ListDirTests : BaseTest
    {
        [TestCleanup]
        public void CleanUp()
        {
            var baseDir = "Foo";
            this.Client.DeleteDir(baseDir);
            for (int i = 0; i < 5; i++)
            {
                var dirName = baseDir + i.ToString();
                this.Client.DeleteDir(dirName);
            }
        }

        [TestMethod]
        public void Test_ListDir_Success()
        {
            var dirName = "Foo";
            this.Client.DeleteDir(dirName);
            this.Client.MakeDir2(dirName);
            var listResults = this.Client.ListDir("/", 100, 0, true);
            ListDirResult foundResult = null;
            foreach (var dir in listResults)
            {
                if (dir.Name == dirName)
                {
                    foundResult = dir;
                }
            }
            Assert.AreEqual(dirName, foundResult.Name);
            Assert.IsTrue(foundResult.Ctime > 0);
            Assert.IsTrue(foundResult.Mtime > 0);
            Assert.IsTrue(foundResult.Gid > 0);
            Assert.IsTrue(foundResult.Uid > 0);
        }

        [TestMethod]
        public void Test_ListDir_Without_Stat()
        {
            var dirName = "Foo";
            this.Client.DeleteDir(dirName);
            this.Client.MakeDir2(dirName);
            var listResults = this.Client.ListDir("/", 10, 0, false);
            ListDirResult foundResult = null;
            foreach (var dir in listResults)
            {
                if (dir.Name == dirName)
                {
                    foundResult = dir;
                }
            }

            Assert.AreEqual(dirName, foundResult.Name);
            Assert.AreEqual(0, foundResult.Ctime);
            Assert.AreEqual(0, foundResult.Mtime);
            Assert.AreEqual(0, foundResult.Gid);
            Assert.AreEqual(0, foundResult.Uid);
        }

        [TestMethod]
        public void Test_ListDir_Cookie_Paging()
        {
            for (int i = 0; i < 5; i++)
            {
                var dirName = "Foo" + i.ToString();
                this.Client.DeleteDir(dirName);
                this.Client.MakeDir2(dirName);
            }
            
            var listResults = this.Client.ListDir("/", 3, 0, true);
            Assert.AreEqual(3, listResults.Count);
            Assert.IsTrue(listResults.Cookie > 0);

            listResults = this.Client.ListDir("/", 1000, listResults.Cookie, true);
            Assert.IsTrue(listResults.Count > 0);
            Assert.AreEqual(0, listResults.Cookie);
        }
    }
}
