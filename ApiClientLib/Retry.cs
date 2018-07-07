using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace ApiClientLib
{
    public interface IRpcRetry
    {
        object Invoke(string method, IArgMaker argMaker, ICodeGetter codeGetter);
    }
    
    public class RpcRetry : IRpcRetry
    {
        private int maxTries = 5;
        private IJsonTransport transport;
        private ILoginManager loginMan;

        public RpcRetry(IJsonTransport transport, ILoginManager loginMan) : this(transport, loginMan, 5) {}
        public RpcRetry(IJsonTransport transport, ILoginManager loginMan, int tries)
        {
            this.transport = transport;
            this.loginMan = loginMan;
            this.maxTries = tries;
        }

        public object Invoke(string method, IArgMaker argMaker, ICodeGetter codeGetter)
        {
            int code = 0;
            for (int i = 0; i < this.maxTries; i++)
            {
                try
                {
                    var login = this.loginMan.Login();
                    var json = this.transport.Invoke(method, argMaker.GetArgs(login.Token));
                    code = codeGetter.GetCode(json);
                    if (code == AgileCode.ExpiredToken)
                    {
                        throw new LoginException(string.Format("Received {0}", code));
                    }
                    else if (code == AgileCode.Success)
                    {
                        return json;
                    }
                    throw new ApiException(code, HttpCode.Ok, "Received non-zero Agile Status");
                }
                catch (LoginException)
                {
                    code = AgileCode.ExpiredToken;
                    this.loginMan.ClearLogin();
                    continue;
                }
            }
            throw new ApiException(code, HttpCode.Ok, string.Format("Failed after {0} tries", this.maxTries));
        }
    }

    public interface IHttpRetry
    {
        WebHeaderCollection Invoke(Uri apiUrl, Stream dataStream, string tag, Dictionary<string, string> headers);
    }

    public class HttpRetry : IHttpRetry
    {
        private int maxTries = 5;
        private IHttpTransport transport;
        private ILoginManager loginMan;

        public HttpRetry(IHttpTransport transport, ILoginManager loginMan) : this(transport, loginMan, 5) { }
        public HttpRetry(IHttpTransport transport, ILoginManager loginMan, int tries)
        {
            this.transport = transport;
            this.loginMan = loginMan;
            this.maxTries = tries;
        }

        public WebHeaderCollection Invoke(Uri apiUrl, Stream dataStream, string tag, Dictionary<string,string> headers) 
        {
            int lastHttpStatus = HttpCode.Forbidden;
            int lastAgileStatus = -1;
            var startPos = dataStream.Position;

            for (int i = 0; i < this.maxTries; i++)
            {
                try
                {
                    var login = this.loginMan.Login();

                    try
                    {
                        return this.transport.Post(apiUrl, login.Token, dataStream, tag, headers);
                    }
                    catch (WebException wex)
                    {
                        dataStream.Position = startPos;
                        var response = (HttpWebResponse)wex.Response;
                        if (response == null)
                        {
                            continue;
                        }
                        else
                        {
                            lastHttpStatus = Convert.ToInt32(response.StatusCode);
                            if (lastHttpStatus == HttpCode.Forbidden || lastHttpStatus == HttpCode.Unauthorized|| lastHttpStatus == HttpCode.InternalError)
                            {
                                throw new LoginException(string.Format("Received {0}", lastHttpStatus));
                            }
                        }
                        lastAgileStatus = Convert.ToInt32(response.Headers.Get("X-Agile-Status"));
                        continue;
                    }                    
                }
                catch (LoginException)
                {
                    this.loginMan.ClearLogin();                                        
                    continue;
                }
            }
            throw new ApiException(lastAgileStatus, lastHttpStatus, string.Format("Failed after {0} tries", this.maxTries));
        }
    }

    public interface IArgMaker
    {
        object[] GetArgs(string token);
    }

    public class RawTokenArgMaker : IArgMaker
    {
        object[] args;

        public RawTokenArgMaker(params object[] args)
        {
            this.args = args;
        }

        public object[] GetArgs(string token)
        {
            return this.args;
        }
    }

    public class AutoTokenArgMaker : IArgMaker
    {
        object[] args;

        public AutoTokenArgMaker(params object[] args)
        {   
            this.args = args;
        }

        public object[] GetArgs(string token)
        {
            var newArgs = new object[this.args.Length + 1];
            newArgs[0] = token;
            args.CopyTo(newArgs, 1);
            return newArgs;
        }
    }

    public interface ICodeGetter
    {
        int GetCode(object json);
    }

    public class CodeGetter : ICodeGetter
    {
        public int GetCode(object json) {
            try
            {
                return ((JToken)json).ToObject<Int32>();
            }
            catch
            {
                return AgileCode.UnknownError;
            }
        }
    }

    public class DictCodeGetter : ICodeGetter
    {
        public int GetCode(object json)
        {
            try
            {
                var oJson = (JObject)json;
                return oJson["code"].ToObject<Int32>();
            }
            catch
            {
                return AgileCode.UnknownError;
            }
        }
    }
}
