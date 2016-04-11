using System;
using System.Collections.Generic;
using System.IO;

namespace ApiClientLib
{
    public class SmartUpload
    {
        public event OnProgressHandler OnProgress;
        private ApiClient Client;
        private SmartUploadProgress progress;

        public SmartUpload(ApiClient client)
        {
            this.Client = client;
            this.Client.OnProgress += Client_OnProgress;
        }

        public string MakeFile(string localPath, string remotePath, int pieceSize)
        {
            return this.MakeFile(localPath, remotePath, pieceSize, new Dictionary<string, string>());
        }
        public string MakeFile(string localPath, string remotePath, int pieceSize, Dictionary<string, string> headers)
        {
            var localFilename = Path.GetFileName(localPath);
            using (var fileStream = File.OpenRead(localPath))
            {
                return this.MakeFile(fileStream, remotePath, pieceSize, headers);
            }
        }
        public string MakeFile(Stream dataStream, string remotePath, int pieceSize, Dictionary<string, string> headers)
        {
            string mpId = null;
            
            this.progress = new SmartUploadProgress(remotePath, dataStream.Length);
           
            if (dataStream.Length <= pieceSize)
            {
                this.Client.MakeFile(dataStream, remotePath, headers);
                return mpId;
            }

            var createResult = this.Client.CreateMultipart(remotePath, headers);
            mpId = createResult.MpId;

            var bytesLeft = dataStream.Length;
            var pieceNum = 0;
            while (bytesLeft > 0)
            {
                using (var tempStream = new MemoryStream(pieceSize))
                {
                    CopyStream(dataStream, tempStream, pieceSize);
                    tempStream.Position = 0;
                    bytesLeft -= tempStream.Length;
                    pieceNum++;
                    this.Client.CreateMultipartPiece(tempStream, mpId, pieceNum);
                }
            }
            this.Client.CompleteMultipart(mpId);
            return mpId;
        }

        void CopyStream(Stream input, Stream output, int bytesToRead)
        {   
            byte[] buffer = new byte[32768];
            int totalRead = 0;
            int toRead = buffer.Length;
         
            while (totalRead < bytesToRead)
            {
                if (bytesToRead - totalRead < buffer.Length)
                {
                    toRead = bytesToRead - totalRead;
                }

                int read = input.Read(buffer, 0, toRead);
                totalRead += read;
                if (read <= 0) break;
                output.Write(buffer, 0, read);
            }
        }

        void Client_OnProgress(object sender, OnProgressArgs args)
        {
            if (this.OnProgress != null && this.progress != null)
            {
                this.OnProgress(this, this.progress.OnProgressHandler(args));
            }
        }
    }

    public class SmartUploadProgress
    {
        private string remotePath;
        private long totalBytes;
        private long totalBytesRead;

        public SmartUploadProgress(string remotePath, long totalBytes)
        {
            this.remotePath = remotePath;
            this.totalBytes = totalBytes;
            this.totalBytesRead = 0;
        }

        public OnProgressArgs OnProgressHandler(OnProgressArgs args)
        {
            this.totalBytesRead += args.TotalBytesRead;
            return new OnProgressArgs(this.remotePath, args.LastBytesRead, this.totalBytesRead, this.totalBytes);
        }
    }
}
