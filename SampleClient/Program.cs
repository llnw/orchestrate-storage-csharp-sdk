using ApiClientLib;
using System;
using System.Net;

namespace SampleClient
{
    class Program
    {

        static void Main(string[] args)
        {
            // Ignore self-signed SSL certs
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;          

            var user = "user";
            var password = "password";
            var url = "https://api.agile.lldns.net";
            var localPath = @"c:\the\path\to\be\uploaded.txt";
            var remotePath = "/uploaded-file.txt";

            var client = new ApiClient(user, password, url);

            // There's no need to login, this is done internally
            var result = client.MakeFile(localPath, remotePath);
            Console.WriteLine("Got result: Size={0}, Checksum={1}, Path={2}", result.Size, result.Checksum, result.Path);

            var uploader = new SmartUpload(client);
            var mpId = uploader.MakeFile(localPath, remotePath, 100);
        }
    }
}
