using Jayrock.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace ApiClientLib
{
    public class LoginResult
    {
        public string Token;
        public int Gid;
        public int Uid;
        public string Path;

        public LoginResult FromJsonObject(JsonArray json)
        {
            if (json.Length == 2 && json[0] == null && json[1] == null)
            {
                throw new LoginException(json.ToString());
            }
            this.Token = (string)json[0];
            var userInfo = (JsonObject)json[1];
            this.Gid = ((JsonNumber)userInfo["gid"]).ToInt32();
            this.Uid = ((JsonNumber)userInfo["uid"]).ToInt32();
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
            var json = (JsonObject)obj;
            this.Code = ((JsonNumber)json["code"]).ToInt32();
            this.Ctime = ((JsonNumber)json["ctime"]).ToInt32();
            this.Checksum = (string)json["checksum"];
            this.Gid = ((JsonNumber)json["gid"]).ToInt32();
            this.Mtime = ((JsonNumber)json["mtime"]).ToInt32();           
            this.Type = ((JsonNumber)json["type"]).ToInt32();
            if (this.Type == 2)
            {
                this.Size = ((JsonNumber)json["size"]).ToInt64();
            }
            this.Uid = ((JsonNumber)json["uid"]).ToInt32();
            return this;
        }
    }

    public class ListDirResults : List<ListDirResult>
    {
        public long Cookie;

        public ListDirResults FromJsonObject(object obj)
        {
            var json = (JsonObject)obj;
            this.Cookie = ((JsonNumber)json["cookie"]).ToInt32();
            var list = (JsonArray)json["list"];
            this.Clear();

            foreach (JsonObject item in list)
            {
                this.Add(new ListDirResult().FromJsonObject(item));
            }
            return this;
        }
    }

    public class ListDirResult : BaseStatData
    {
        public string Name;

        public ListDirResult FromJsonObject(JsonObject json)
        {
            var stat = (JsonObject)json["stat"];
            this.Name = (string)json["name"];

            var oType = json.Contains("type") ? ((JsonNumber)json["type"]).ToInt32() : 2;
            
            if (stat != null)
            {
                this.Ctime = ((JsonNumber)stat["ctime"]).ToInt32();
                this.Gid = ((JsonNumber)stat["gid"]).ToInt32();
                this.Mtime = ((JsonNumber)stat["mtime"]).ToInt32();
                this.Uid = ((JsonNumber)stat["uid"]).ToInt32();
            }
            return this;
        }
    }

    public class ListFileResults : List<ListFileResult>
    {
        public long Cookie;

        public ListFileResults FromJsonObject(object obj)
        {
            var json = (JsonObject)obj;
            this.Cookie = ((JsonNumber)json["cookie"]).ToInt32();
            var list = (JsonArray)json["list"];
            this.Clear();

            foreach (JsonObject item in list)
            {
                this.Add(new ListFileResult().FromJsonObject(item));
            }
            return this;
        }
    }

    public class ListFileResult : BaseListFileData
    {
        public string ContentType;

        public ListFileResult FromJsonObject(JsonObject json)
        {
            var stat = (JsonObject)json["stat"];
            this.Name = (string)json["name"];
            this.Type = ((JsonNumber)json["type"]).ToInt32();

            if (stat != null)
            {
                this.Checksum = (string)stat["checksum"];
                this.Ctime = ((JsonNumber)stat["ctime"]).ToInt32();
                this.Gid = ((JsonNumber)stat["gid"]).ToInt32();
                this.ContentType = (string)stat["mimetype"];
                this.Mtime = ((JsonNumber)stat["mtime"]).ToInt32();
                this.Size = ((JsonNumber)stat["size"]).ToInt64();
                this.Uid = ((JsonNumber)stat["uid"]).ToInt32();
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
            var json = (JsonObject)obj;
            this.Cookie = (string)json["cookie"];
         
            var dirs = (JsonArray)json["dirs"];
            
            foreach (JsonObject dir in dirs)
            {
                var dirResult = new ListDirResult();
                dirResult.FromJsonObject(dir);
                this.Dirs.Add(dirResult);
            }

            var files = (JsonArray)json["files"];

            foreach (JsonObject file in files)
            {
                this.Files.Add(new ListPathFileResult().FromJsonObject(file));
            }
            return this;
        }
    }

    public class ListPathFileResult : BaseListFileData
    {
        public int ContentType;

        public ListPathFileResult FromJsonObject(JsonObject json)
        {
            var stat = (JsonObject)json["stat"];
            this.Name = (string)json["name"];
           
            if (stat != null)
            {
                this.Checksum = (string)stat["hash"];
                this.Ctime = ((JsonNumber)stat["ctime"]).ToInt32();
                this.ContentType = ((JsonNumber)stat["ctype"]).ToInt32();                
                this.Gid = ((JsonNumber)stat["gid"]).ToInt32();
                this.Mtime = ((JsonNumber)stat["mtime"]).ToInt32();
                this.Size = ((JsonNumber)stat["size"]).ToInt64();
                this.Uid = ((JsonNumber)stat["uid"]).ToInt32();
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
            var json = (JsonObject)obj;
            this.Created = ((JsonNumber)json["created"]).ToInt32();
            this.Mtime = ((JsonNumber)json["mtime"]).ToInt32();
            this.NumPieces = ((JsonNumber)json["numpieces"]).ToInt32();
            this.State = ((JsonNumber)json["state"]).ToInt32();
            this.ContentType = (string)json["content_type"];
            this.Error = ((JsonNumber)json["error"]).ToInt32();
            this.Path = (string)json["path"];
            return this;
        }
    }

    public class MultipartResults : List<MultipartResult>
    {
        public int Cookie;

        public MultipartResults FromJsonObject(object obj)
        {
            var json = (JsonObject)obj;
            this.Cookie = ((JsonNumber)json["cookie"]).ToInt32();

            var list = (JsonArray)json["multipart"];

            foreach (JsonObject item in list)
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

        public MultipartResult FromJsonObject(JsonObject json)
        {        
            this.Error = ((JsonNumber)json["error"]).ToInt32();
            this.Gid = ((JsonNumber)json["gid"]).ToInt32();
            this.MpId = (string)json["mpid"];
            this.Mtime = ((JsonNumber)json["mtime"]).ToInt32();
            this.Path = (string)json["path"];            
            this.State = ((JsonNumber)json["state"]).ToInt32();
            this.Uid = ((JsonNumber)json["uid"]).ToInt32();                    
            return this;
        }
    }

    public class MultipartPieceResults : List<MultipartPieceResult>
    {
        public int Cookie;

        public MultipartPieceResults FromJsonObject(object obj)
        {
            var json = (JsonObject)obj;
            this.Cookie = ((JsonNumber)json["cookie"]).ToInt32();

            var list = (JsonArray)json["pieces"];

            foreach (JsonObject item in list)
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

        public MultipartPieceResult FromJsonObject(JsonObject json)
        {
            this.Error = ((JsonNumber)json["error"]).ToInt32();
            this.Number = ((JsonNumber)json["number"]).ToInt32();
            this.State = ((JsonNumber)json["state"]).ToInt32();
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
