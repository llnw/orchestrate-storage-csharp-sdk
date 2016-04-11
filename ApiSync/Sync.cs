using ApiClientLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ApiSync
{
    class Sync
    {
        ApiClient client;
        Config config;
        bool pretend;
        Semaphore pool;
        int concurrency;

        public Sync(ApiClient client, Config config, int concurrency, bool pretend)
        {
            this.client = client;
            this.config = config;
            this.pretend = pretend;
            this.concurrency = concurrency;
            this.pool = new Semaphore(concurrency, concurrency);
        }

        public void Start()
        {
            try
            {
                this.client.Login();

            }
            catch (LoginException)
            {
                Console.WriteLine("Failed to login");
                return;
            }

            this.SyncDirectory(config.FromPath, config.ToPath);
            for (int i = 0; i < this.concurrency; i++)
            {
                this.pool.WaitOne();
            }
        }

        private void SyncDirectory(string syncDir, string remotePath)
        {            
            var dirName = new DirectoryInfo(syncDir).Name;
            remotePath = string.Format("{0}/{1}", remotePath, dirName);

            this.CreateDirectoryIfNotExists(remotePath);

            this.SyncFiles(syncDir, remotePath);

            var dirs = Directory.EnumerateDirectories(syncDir);
            foreach (var dir in dirs)
            {
                this.SyncDirectory(dir, remotePath);
            }
        }

        private void SyncFiles(string syncDir, string remotePath)
        {
            var files = Directory.EnumerateFiles(syncDir);

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                var remoteFile = string.Format("{0}/{1}", remotePath, fileInfo.Name);

                this.CreateFileIfNotExists(remoteFile, fileInfo);
            }
        }

        private void CreateFileIfNotExists(string remotePath, FileInfo info)
        {
            this.pool.WaitOne();
            var fileData = new FileData(remotePath, info);
            var worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync(fileData);
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.pool.Release();
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            FileData fileData = (FileData)e.Argument;
            this.CreateFileIfNotExists2(fileData.RemotePath, fileData.Info);
        }

        private void CreateFileIfNotExists2(string remotePath, FileInfo info)
        {
            try
            {
                var statData = this.GetStatData(remotePath);
                if (statData != null && statData.Size == info.Length)
                {
                    Console.WriteLine("Skipping existing file {0}", remotePath);
                    return;
                }

                if (this.pretend)
                {
                    Console.WriteLine("Skipping. Create file {0} with size {1}", remotePath, info.Length);
                    return;
                }

                var makeFileResult = this.client.MakeFile(info.FullName, remotePath);
                if (makeFileResult.Status == 0)
                {
                    Console.WriteLine("Create file {0} with size {1}", remotePath, info.Length);
                    return;
                }
                throw new ApiException(makeFileResult.Status, 200, String.Empty);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private StatResult GetStatData(string remotePath)
        {
            try
            {
                return this.client.Stat(remotePath);
            }
            catch (ApiException ex)
            {
                if (ex.AgileStatusCode == -1)
                {
                    return null;
                }
                throw ex;
            }
        }

        private void CreateDirectoryIfNotExists(string remotePath)
        {
            var statData = this.GetStatData(remotePath);
            if (statData == null)
            {
                try
                {
                    if (!this.pretend)
                    {
                        this.client.MakeDir2(remotePath);
                        this.CreateDirectoryIfNotExists(remotePath);
                        Console.WriteLine("Created directory {0}", remotePath);
                    }
                }
                catch (ApiException ex2)
                {
                    Console.WriteLine("Aborting. Failed to create directory {0} - {1}", remotePath, ex2);
                    throw ex2;
                }
            }
        }
    }

    public class FileData
    {
        public string RemotePath;
        public FileInfo Info;

        public FileData(string remotePath, FileInfo info)
        {
            this.RemotePath = remotePath;
            this.Info = info;
        }
    }
}
