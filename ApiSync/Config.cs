using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace ApiSync
{
    class Config
    {
        public string FromPath;
        public string ToPath;
        public Config(string[] args)
        {
            this.FromPath = args[0];
            this.ToPath = args[1];
        }

        public string GetApiUrl() { return ConfigurationManager.AppSettings["ApiUrl"]; }
        public string GetUser() { return ConfigurationManager.AppSettings["ApiUser"]; }
        public string GetPassword() { return ConfigurationManager.AppSettings["ApiPassword"]; }
    }
}
