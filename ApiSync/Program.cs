using ApiClientLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ApiSync
{
    class Program
    {
        /* TODO
         * Multithreading
         * Track how long the sync takes
         */

        static void Main(string[] args)
        {
            var pretend = false;
            var concurrency = 100;

            Config config;
            try 
            {
                config= GetConfig(args); 
            }
            catch {
                return;
            }
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            var client = new ApiClient(config.GetUser(), config.GetPassword(), config.GetApiUrl());
            if (!ValidateLogin(client)) {
                return;
            }
            
            var sync = new Sync(client, config, concurrency, pretend);
            Console.WriteLine("Starting Sync for {2} on {3} with {0} -> {1}", config.FromPath, config.ToPath, config.GetUser(), config.GetApiUrl());
            try
            {
                var watch = new Stopwatch();
                watch.Start();
                sync.Start();
                watch.Stop();
                Console.WriteLine("Sync took {0} seconds", watch.Elapsed.TotalSeconds);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to sync. {0}", ex);
            }
        }

        static Config GetConfig(string[] args)
        {
            if (args.Length < 2)
            {
                DisplayUsage();
                throw new ArgumentException();
            }
            return new Config(args);
        }

        static void DisplayUsage()
        {
            Console.WriteLine("Usage: apisync.exe path_to_sync");
            Console.WriteLine("Example: apisync.exec C:\\my\\files\\");
        }

        static bool ValidateLogin(ApiClient client) {
            try
            {
                client.Login();
                return true;
            }
            catch (LoginException)
            {
                Console.WriteLine("Failed to login");
                return false;
            }
        }
    }

}
