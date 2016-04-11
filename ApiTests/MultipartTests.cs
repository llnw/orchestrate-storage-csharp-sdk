using ApiClientLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace ApiTests
{
    [TestClass]
    public class MultipartTests : BaseTest
    {
        [TestMethod]
        public void Test_Multipart_Do_It()
        {
            var remotePath = string.Format("/multipartfile-{0}.txt", System.Guid.NewGuid().ToString());
            var contentType = "text/plain";
            var mtime = 12345;
            var headers = new Dictionary<string, string>() {
                {"X-Agile-Content-Type", contentType},
                {"X-Agile-MTime", mtime.ToString()},
            };

            this.Client.OnProgress += Client_OnProgress;
            var createResult = this.Client.CreateMultipart(remotePath, headers);
            var mpId = createResult.MpId;
            var stream = new MemoryStream(new byte[] { 1,2,3,4,5,6 });
            var pieceResult = this.Client.CreateMultipartPiece(stream, mpId, 1);
            Assert.AreEqual(0, pieceResult.Status);
            Assert.AreEqual(6, pieceResult.Size);

            var pieces = this.Client.ListMultipartPiece(mpId, 10, 0);
            Assert.AreEqual(1, pieces.Count);

            var stats = this.Client.GetMultipartStatus(mpId);
            Assert.IsTrue(stats.Created > 0);
            Assert.AreEqual(mtime, stats.Mtime);
            Assert.AreEqual(1, stats.NumPieces);
            Assert.AreEqual(2, stats.State);
            Assert.AreEqual(contentType, stats.ContentType);
            Assert.AreEqual(0, stats.Error);
            Assert.AreEqual(this.Client.GetAgilePath() + remotePath, stats.Path);

            this.Client.CompleteMultipart(mpId);

            var list = this.Client.ListMultipart(10, 0);
            Assert.IsTrue(list.Count > 0);

            this.Client.DeleteFile(remotePath);  
        }

        void Client_OnProgress(object sender, OnProgressArgs args)
        {
            Console.WriteLine("Progress File={0} LastBytes={1} TotalBytes={2} TotalRead={3}", args.Tag, args.LastBytesRead, args.TotalBytesRead, args.TotalBytes);
        }

        [TestMethod]
        public void Test_Multipart_Abort()
        {
            var remotePath = string.Format("/multipartfile-{0}.txt", System.Guid.NewGuid().ToString());
            var contentType = "text/plain";
            var mtime = 12345;
            var headers = new Dictionary<string,string>() {
                {"X-Agile-Content-Type", contentType},
                {"X-Agile-MTime", mtime.ToString()},
            };
            var createResult = this.Client.CreateMultipart(remotePath, headers);
            this.Client.AbortMultipart(createResult.MpId);
        }

        [TestMethod]
        public void Test_Multipart_List()
        {
            var list = this.Client.ListMultipart(100, 0);
        }

        [TestMethod]
        public void Test_Make_Multipart_file()
        {
            var size = 100;
            var sb = new StringBuilder();
            for (int i = 0; i < size; i++)
            {
                sb.Append(i.ToString());
                sb.Append("-");
            }
            var localPath = this.Fixture.CreateFile(sb.ToString());
            var remotePath = string.Format("/multipartfile-{0}.txt", System.Guid.NewGuid().ToString());
            var chunkSize = 50;

            // Create the multipart file
            var smart = new SmartUpload(this.Client);
            smart.OnProgress += Client_OnProgress;

            var mpId = smart.MakeFile(localPath, remotePath, chunkSize);
           
            // Calculate the hexlified version of the checksum
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var hasher = SHA256.Create();
            var hash = hasher.ComputeHash(bytes);
            var strHexBuilder = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                strHexBuilder.AppendFormat("{0:x2}", hash[i]);
            }
            var strHash = strHexBuilder.ToString();

            // Wait for the consumers to put together the multipart file
            while (true)
            {
                var status = this.Client.GetMultipartStatus(mpId);
                if (status.State == 6)
                {
                    break;
                }
                Thread.Sleep(200);               
            }

            // Request the file
            var url = string.Format("http://global.mt.lldns.net/{0}{1}", this.Client.GetAgilePath(), remotePath);
            WebRequest request = null;

            var tries = 10;
            for (int i = 0; i < tries; i++)
            {
                try
                {
                    request = HttpWebRequest.Create(url);
                }
                catch (WebException)
                {
                    Thread.Sleep(1000);
                }
            }
            using (var response = request.GetResponse())
            {
                var fileChecksum = response.Headers.Get("X-Agile-Checksum");
                var contentLength = Convert.ToInt32(response.Headers.Get("Content-Length"));
                using (var respStream = response.GetResponseStream())
                {
                    var buffer = new byte[contentLength];
                    var read = respStream.Read(buffer, 0, contentLength);
                    var bufferStr = Encoding.UTF8.GetString(buffer);
                    
                    // Confirm that the contents of the file match what we sent
                    Assert.AreEqual(sb.ToString(), bufferStr);
                    // Confirm the locally calculated checksum matches the checksum returned when GET'ing the file
                    Assert.AreEqual(strHash, fileChecksum);
                }
            }
        }
    }
}
