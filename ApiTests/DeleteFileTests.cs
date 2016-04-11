using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace ApiTests
{
    [TestClass]
    public class DeleteFileTests : BaseTest
    {
        [TestMethod]
        public void Test_DeleteFile_Success()
        {
            var localPath = this.Fixture.CreateFile("Foo");
            var remotePath = "/" + Path.GetFileName(localPath);
            
            var result = this.Client.MakeFile(localPath, remotePath);
            this.Client.DeleteFile(remotePath);
        }

        [TestMethod]
        public void Test_DeleteFile_Success_Doesnt_Exist()
        {
            var remotePath = "/NotARealFile.txt";
            this.Client.DeleteFile(remotePath);
        }
    }
}
