using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;

namespace ApiClientLib
{
    public class LoginResult
    {
        public string Token;
        public int Gid;
        public int Uid;
        public string Path;

        public LoginResult FromJsonObject(JArray json)
        {
            if (json.Count == 2 && json[0] == null && json[1] == null)
            {
                throw new LoginException(json.ToString());
            }
            this.Token = (string)json[0];
            var userInfo = json[1].ToObject<JObject>();
            this.Gid = userInfo["gid"].ToObject<Int32>();
            this.Uid = userInfo["uid"].ToObject<Int32>();
            this.Path = (string)userInfo["path"];
            return this;
        }
    }

    public class MakeFileResult
    {
        public string Path;
        public int Status;
        public Int64 Size;
        public string Checksum;

        public MakeFileResult FromHttpHeaders(WebHeaderCollection headers)
        {   
            this.Path = headers.Get("X-Agile-Path");
            this.Status = Convert.ToInt32(headers.Get("X-Agile-Status"));
            this.Size = Convert.ToInt64(headers.Get("X-Agile-Size"));
            this.Checksum = headers.Get("X-Agile-Checksum");
            return this;
        }
    }

    public class BaseStatData
    {
        public int Ctime;
        public int Gid;
        public int Mtime;
        public int Type;
        public int Uid;
    }

    public class BaseListFileData : BaseStatData
    {
        public string Checksum;
        public string Name;
        public Int64 Size;
    }

    public class StatResult : BaseStatData
    {
        public int Code;
        public string Checksum;
        public Int64 Size;

        public StatResult FromJsonObject(object obj)
        {
            var json = (JObject)obj;
            this.Code = json["code"].ToObject<Int32>();
            this.Ctime = json["ctime"].ToObject<Int32>();
            this.Checksum = (string)json["checksum"];
            this.Gid = json["gid"].ToObject<Int32>();
            this.Mtime = json["mtime"].ToObject<Int32>();
            this.Type = json["type"].ToObject<Int32>();
            if (this.Type == 2)
            {
                this.Size = json["size"].ToObject<Int64>();
            }
            this.Uid = json["uid"].ToObject<Int32>();
            return this;
        }
    }

    public class ListDirResults : List<ListDirResult>
    {
        public int Cookie;

        public ListDirResults FromJsonObject(object obj)
        {
            var json = (JObject)obj;
            this.Cookie = json["cookie"].ToObject<Int32>();
            var list = (JArray)json["list"];
            this.Clear();

            foreach (JObject item in list)
            {
                this.Add(new ListDirResult().FromJsonObject(item));
            }
            return this;
        }
    }

    public class ListDirResult : BaseStatData
    {
        public string Name;

        public ListDirResult FromJsonObject(JObject json)
        {
            var stat = (JObject)json["stat"];
            this.Name = (string)json["name"];
            JToken typeToken = null;
            var oType = 2;
            if (json.TryGetValue("type", out typeToken))
                oType = json["type"].ToObject<Int32>();
            
            if (stat.Type != JTokenType.Null)
            {
                this.Ctime = stat["ctime"].ToObject<Int32>();
                this.Gid = stat["gid"].ToObject<Int32>();
                this.Mtime = stat["mtime"].ToObject<Int32>();
                this.Uid = stat["uid"].ToObject<Int32>();
            }
            return this;
        }
    }

    public class ListFileResults : List<ListFileResult>
    {
        public int Cookie;

        public ListFileResults FromJsonObject(object obj)
        {
            var json = (JObject)obj;
            this.Cookie = json["cookie"].ToObject<Int32>();
            var list = (JArray)json["list"];
            this.Clear();

            foreach (JObject item in list)
            {
                this.Add(new ListFileResult().FromJsonObject(item));
            }
            return this;
        }
    }

    public class ListFileResult : BaseListFileData
    {
        public string ContentType;

        public ListFileResult FromJsonObject(JObject json)
        {
            var stat = (JObject)json["stat"];
            this.Name = (string)json["name"];
            this.Type = json["type"].ToObject<Int32>();

            if (stat.Type != JTokenType.Null)
            {
                this.Checksum = (string)stat["checksum"];
                this.Ctime = stat["ctime"].ToObject<Int32>();
                this.Gid = stat["gid"].ToObject<Int32>();
                this.ContentType = (string)stat["mimetype"];
                this.Mtime = stat["mtime"].ToObject<Int32>();
                this.Size = stat["size"].ToObject<Int64>();
                this.Uid = stat["uid"].ToObject<Int32>();
            }
            return this;
        }
    }

    public class ListPathResults
    {
        public string Cookie;
        public List<ListPathFileResult> Files = new List<ListPathFileResult>();
        public List<ListDirResult> Dirs = new List<ListDirResult>();

        public ListPathResults FromJsonObject(object obj)
        {
            var json = (JObject)obj;
            this.Cookie = (string)json["cookie"];
         
            var dirs = (JArray)json["dirs"];
            
            foreach (JObject dir in dirs)
            {
                var dirResult = new ListDirResult();
                dirResult.FromJsonObject(dir);
                this.Dirs.Add(dirResult);
            }

            var files = (JArray)json["files"];

            foreach (JObject file in files)
            {
                this.Files.Add(new ListPathFileResult().FromJsonObject(file));
            }
            return this;
        }
    }

    public class ListPathFileResult : BaseListFileData
    {
        public int ContentType;

        public ListPathFileResult FromJsonObject(JObject json)
        {
            var stat = (JObject)json["stat"];
            this.Name = (string)json["name"];
           
            if (stat.Type != JTokenType.Null)
            {
                this.Checksum = (string)stat["hash"];
                this.Ctime = stat["ctime"].ToObject<Int32>();
                this.ContentType = stat["ctype"].ToObject<Int32>();
                this.Gid = stat["gid"].ToObject<Int32>();
                this.Mtime = stat["mtime"].ToObject<Int32>();
                this.Size = stat["size"].ToObject<Int64>();
                this.Uid = stat["uid"].ToObject<Int32>();
            }
            return this;
        }
    }

    public class MultipartInfo
    {
        public int Created;
        public int State;
        public int Error;
        public int NumPieces;
        public string Path;
        public string ContentType;
        public int Mtime;


        public MultipartInfo FromJsonObject(object obj)
        {
            var json = (JObject)obj;
            this.Created = json["created"].ToObject<Int32>();
            this.Mtime = json["mtime"].ToObject<Int32>();
            this.NumPieces = json["numpieces"].ToObject<Int32>();
            this.State = json["state"].ToObject<Int32>();
            this.ContentType = (string)json["content_type"];
            this.Error = json["error"].ToObject<Int32>();
            this.Path = (string)json["path"];
            return this;
        }
    }

    public class MultipartResults : List<MultipartResult>
    {
        public int Cookie;

        public MultipartResults FromJsonObject(object obj)
        {
            var json = (JObject)obj;
            this.Cookie = json["cookie"].ToObject<Int32>();

            var list = (JArray)json["multipart"];

            foreach (JObject item in list)
            {
                var mpResult = new MultipartResult();
                mpResult.FromJsonObject(item);
                this.Add(mpResult);
            }

            return this;
        }
    }

    public class MultipartResult
    {
        public int Error;
        public int Gid;
        public string MpId;
        public int Mtime;
        public string Path;
        public int State;
        public int Uid;

        public MultipartResult FromJsonObject(JObject json)
        {        
            this.Error = json["error"].ToObject<Int32>();
            this.Gid = json["gid"].ToObject<Int32>();
            this.MpId = (string)json["mpid"];
            this.Mtime = json["mtime"].ToObject<Int32>();
            this.Path = (string)json["path"];
            this.State = json["state"].ToObject<Int32>();
            this.Uid = json["uid"].ToObject<Int32>();
            return this;
        }
    }

    public class MultipartPieceResults : List<MultipartPieceResult>
    {
        public int Cookie;

        public MultipartPieceResults FromJsonObject(object obj)
        {
            var json = (JObject)obj;
            this.Cookie = json["cookie"].ToObject<Int32>();

            var list = (JArray)json["pieces"];

            foreach (JObject item in list)
            {
                var mpResult = new MultipartPieceResult();
                mpResult.FromJsonObject(item);
                this.Add(mpResult);
            }

            return this;
        }
    }

    public class MultipartPieceResult
    {
        public int Error;
        public int Number;
        public int State;

        public MultipartPieceResult FromJsonObject(JObject json)
        {
            this.Error = json["error"].ToObject<Int32>();
            this.Number = json["number"].ToObject<Int32>();
            this.State = json["state"].ToObject<Int32>();
            return this;
        }
    }

    public class CreateMultipartPieceResult
    {
        public int Status;
        public Int64 Size;
        public string Checksum;

        public CreateMultipartPieceResult FromHttpHeaders(WebHeaderCollection headers)
        {
            this.Status = Convert.ToInt32(headers.Get("X-Agile-Status"));
            this.Size = Convert.ToInt64(headers.Get("X-Agile-Size"));
            this.Checksum = headers.Get("X-Agile-Checksum");
            return this;
        }
    }

    public class CreateMultipartResult
    {
        public int Status;
        public string MpId;
        public string Path;

        public CreateMultipartResult FromHttpHeaders(WebHeaderCollection headers)
        {
            this.Status = Convert.ToInt32(headers.Get("X-Agile-Status"));
            this.MpId = headers.Get("X-Agile-MultiPart");
            this.Path = headers.Get("X-Agile-Path");
            return this;
        }
    }

    public class Urls
    {
        public string PostRaw;
        public string PostForm;
        public string RpcUrl;
        public string MpPiece;
        public string MpCreate;
        public string MpComplete;

        public Urls(string apiUrl)
        {
            this.PostRaw = apiUrl + "/post/raw";
            this.PostForm = apiUrl + "/post/file";
            this.RpcUrl = apiUrl + "/jsonrpc";
            this.MpPiece = apiUrl + "/multipart/piece";
            this.MpCreate = apiUrl + "/multipart/create";
            this.MpComplete = apiUrl + "/multipart/complete";
        }
    }

    public static class AgileCode
    {
        public static int Success = 0;
        public static int DirNotFound = -1;
        public static int FileNotFound = -1;
        public static int PathExists = -2;
        public static int NoParentDir = -3;
        public static int ServiceUnavailable = -5;
        public static int InvalidPath = -8;
        public static int UnknownError = -999;
        public static int ExpiredToken = -10001;
    }

    public static class HttpCode
    {
        public static int Ok = 200;
        public static int Unauthorized = 401;
        public static int Forbidden = 403;
        public static int NotFound = 404;
        public static int InternalError = 500;
    }
}
