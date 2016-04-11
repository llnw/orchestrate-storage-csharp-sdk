using ApiClientLib;
using Jayrock.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;

namespace ApiTests
{
    public class TestFixture
    {
        public string GetApiUrl() { return ConfigurationManager.AppSettings["ApiUrl"]; }
        public string GetUser() { return ConfigurationManager.AppSettings["ApiUser"]; }
        public string GetPassword() { return ConfigurationManager.AppSettings["ApiPassword"]; }

        public ApiClient GetClient()
        {
            return this.GetClient(this.GetUser(), this.GetPassword(), this.GetApiUrl());
        } 

        public ApiClient GetClient(string user, string password)
        {
            return this.GetClient(user, password, this.GetApiUrl());
        }

        public ApiClient GetClient(string user, string password, string apiUrl)
        {
            return new ApiClient(user, password, apiUrl);
        } 

        public string CreateFile(string data) {
            var localPath = Path.GetTempFileName();
            using (StreamWriter sw = new StreamWriter(localPath))
            {
                sw.Write(data);
            }
            return localPath;
        }

        public void MakeAnyFile(ApiClient client)
        {
            var localPath = this.CreateFile("Foo");
            var remotePath = "/" + Path.GetFileName(localPath);

            var loginResult = client.Login();
            var result = client.MakeFile(localPath, remotePath);
            Assert.AreEqual(0, result.Status);
        }

        public int GetDefaultFileSize() {
            return 3;
        }

        public string GetDefaultChecksum()
        {
            return "1cbec737f863e4922cee63cc2ebbfaafcd1cff8b790d8cfd2e6a5d550b648afa";
        }

        public JsonArray GetGoodLoginJson()
        {
            return this.GetGoodLoginJson("token");
        }
        public JsonArray GetGoodLoginJson(string token)
        {
            var json = "[\"" + token + "\", { \"gid\": 100, \"uid\": 1001, \"path\": \"/path\" }]";
            var ret = new JsonArray();
            var reader = new JsonTextReader(new StringReader(json));
            ret.Import(reader);
            return ret;
        }

        public JsonArray GetBadLoginJson()
        {
            var json = "[null, null]";
            var ret = new JsonArray();
            var reader = new JsonTextReader(new StringReader(json));
            ret.Import(reader);
            return ret;
        }
    }

    public class FakeCodeGetter : ICodeGetter
    {

        public int Code;

        public FakeCodeGetter(int code)
        {
            this.Code = code;
        }

        public int GetCode(object json)
        {
            return this.Code;
        }
    }

    public class FakeJsonTransport : IJsonTransport, IHttpTransport
    {
        List<object> returns;
        int retIndex;

        public FakeJsonTransport(object retValue)
            : this()
        {
            this.SetReturn(retValue);
        }
        public FakeJsonTransport()
        {
            this.returns = new List<object>();
            this.retIndex = 0;
        }

        public object Invoke(string method, params object[] args)
        {
            var ret = this.returns[this.retIndex];
            this.retIndex++;
            return ret;
        }

        public WebHeaderCollection Post(string apiUrl, string token, Stream dataStream, string tag, Dictionary<string, string> headers)
        {
            var ret = this.returns[this.retIndex];
            this.retIndex++;
            return (WebHeaderCollection)ret;
        }

        public void SetReturn(object ret)
        {
            this.returns.Add(ret);
        }
    }

    public class NullCodeGetter : ICodeGetter
    {
        public int GetCode(object json)
        {
            return AgileCode.Success;
        }
    }
}
