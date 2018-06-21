using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace ApiClientLib
{
    public class ApiClient
    {
        private Urls urls;
        private IRpcRetry rpcRetry;
        private IHttpRetry httpRetry;
        private ILoginManager LoginManager;

        public event OnProgressHandler OnProgress;

        public ApiClient(string user, string password, string apiUrl) : this(user, password, apiUrl, 30000) { }
        public ApiClient(string user, string password, string apiUrl, int timeout)
        {
            this.urls = new Urls(apiUrl);
            var jsonTransport = new JsonTransport(this.urls.RpcUrl, timeout);
            var httpTransport = new HttpTransport(timeout);
            httpTransport.OnProgress += httpTransport_OnProgress;
            this.LoginManager = new LoginManager(user, password, jsonTransport);
            this.rpcRetry = new RpcRetry(jsonTransport, this.LoginManager);
            this.httpRetry = new HttpRetry(httpTransport, this.LoginManager);
        }

        public ApiClient(string apiUrl, IRpcRetry rcpRetry, IHttpRetry httpRetry, ILoginManager loginMan)
        {
            this.urls = new Urls(apiUrl);
            this.LoginManager = loginMan;
            this.rpcRetry = rcpRetry;
            this.httpRetry = httpRetry;
        }

        /// <summary>
        /// Aborts a multipart file
        /// </summary>
        /// <param name="mpId">multipart id</param>
        public void AbortMultipart(string mpId)
        {
            this.InvokeCodeBasedMethod("abortMultipart", mpId);
        }

        /// <summary>
        /// Completes a multipart file
        /// </summary>
        /// <param name="mpId">multipart id</param>
        /// <returns>The number of pieces in the multipart file</returns>
        public int CompleteMultipart(string mpId)
        {
            try
            {
                var argMaker = new AutoTokenArgMaker(mpId);
                var codeGetter = new DictCodeGetter();
                var result = this.rpcRetry.Invoke("completeMultipart", argMaker, codeGetter);

                var dict = (JObject)result;
                return dict.GetValue("numpieces").ToObject<int>();
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnknownApiException(string.Format("getMultipartStatus failed with mpId={0}", mpId), ex);
            }
        }

        /// <summary>
        /// Creates a new multipart file
        /// </summary>
        /// <param name="remotePath">the path and filename of the new file (e.g. /customer/my-file.txt)</param>
        /// <param name="contentType">content type (e.g. text/plain)</param>
        /// <param name="mtime">modified time timestamp.  Seconds since 1970-01-01. </param>
        /// <returns>the multipart id of the new file</returns>
        public CreateMultipartResult CreateMultipart(string remotePath, Dictionary<string, string> headers)
        {
            if (headers == null)
            {
                headers = new Dictionary<string, string>();
            }

            AddDirectoryAndBasename(remotePath, headers);
            var dataStream = new MemoryStream();

            var respHeaders = this.httpRetry.Invoke(this.urls.MpCreate, dataStream, remotePath, headers);
            var result = new CreateMultipartResult().FromHttpHeaders(respHeaders);
            if (result.Status != 0)
            {
                throw new ApiException(result.Status, HttpCode.Ok, string.Format("createMultipart failed with remotePath={0}, headers={1}",
                    remotePath, headers));
            }
            return result;
        }

        /// <summary>
        /// Adds a piece to a multipart file
        /// </summary>
        /// <param name="dataStream">stream containing data to be added</param>
        /// <param name="mpId">multipart id</param>
        /// <param name="number">index of the part within the multipart file.  indexes start at one.</param>
        public CreateMultipartPieceResult CreateMultipartPiece(Stream dataStream, string mpId, int number)
        {
            var headers = new Dictionary<string, string> {
                {"X-Agile-Multipart", mpId},
                {"X-Agile-Part", number.ToString()}
            };

            var tag = string.Format("{0}-{1}", mpId, number);
            var respHeaders = this.httpRetry.Invoke(this.urls.MpPiece, dataStream, tag, headers);
            var result = new CreateMultipartPieceResult().FromHttpHeaders(respHeaders);
            if (result.Status != 0)
            {
                throw new ApiException(result.Status, HttpCode.Ok, string.Format("createMultipartPiece failed with mpId={0}, number={1}",
                    mpId, number));
            }
            return result;
        }

        /// <summary>
        /// returns the status of a multipart file
        /// </summary>
        /// <param name="mpId">multipart id</param>
        public MultipartInfo GetMultipartStatus(string mpId)
        {
            try
            {
                var argMaker = new AutoTokenArgMaker(mpId);
                var codeGetter = new DictCodeGetter();
                var result = this.rpcRetry.Invoke("getMultipartStatus", argMaker, codeGetter);

                var mpResult = new MultipartInfo();
                mpResult.FromJsonObject(result);
                return mpResult;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnknownApiException(string.Format("getMultipartStatus failed with mpId={0}", mpId), ex);
            }
        }

        /// <summary>
        /// lists all multipart files
        /// </summary>
        /// <param name="pageOffset">offset from which to start listing items</param>
        /// <param name="pageSize">number of items per page</param>
        public MultipartResults ListMultipart(int pageSize, int pageOffset)
        {
            try
            {
                var argMaker = new AutoTokenArgMaker(pageOffset, pageSize);
                var codeGetter = new DictCodeGetter();
                var result = this.rpcRetry.Invoke("listMultipart", argMaker, codeGetter);

                var results = new MultipartResults();
                results.FromJsonObject(result);
                return results;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnknownApiException(string.Format("listMultipart failed with pageOffset={0}, pageSize={1}", 
                    pageOffset, pageSize), ex);
            }
        }

        /// <summary>
        /// lists multipart pieces within a multipart file
        /// </summary>
        /// <param name="mpId">multipart id</param>
        /// <param name="pageOffset">offset from which to start listing items</param>
        /// <param name="pageSize">number of items per page</param>
        public MultipartPieceResults ListMultipartPiece(string mpId, int pageSize, int pageOffset)
        {
            try
            {
                var argMaker = new AutoTokenArgMaker(mpId, pageOffset, pageSize);
                var codeGetter = new DictCodeGetter();
                var result = this.rpcRetry.Invoke("listMultipartPiece", argMaker, codeGetter);

                var results = new MultipartPieceResults();
                results.FromJsonObject(result);
                return results;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnknownApiException(string.Format("listMultipartPiece failed with mpId={0}, pageOffset={1}, pageSize={2}",
                    mpId, pageOffset, pageSize), ex);
            }
        }

        /// <summary>
        /// restarts a multipart file
        /// </summary>
        /// <param name="mpId">multipart id</param>
        public void RestartMultipart(string mpId)
        {
            this.InvokeCodeBasedMethod("restartMultipart", mpId);
        }

        /// <summary>
        /// copies a file
        /// </summary>
        /// <param name="fromRemotePath">the path and filename of the original file</param>
        /// <param name="toRemotePath">the path and filename of the new file</param>
        public void CopyFile(string fromRemotePath, string toRemotePath)
        {
            this.InvokeCodeBasedMethod("copyFile", fromRemotePath, toRemotePath);
        }

        /// <summary>
        /// deletes a directory
        /// 
        /// errors are silently ignored
        /// </summary>
        /// <param name="remotePath">the full path to the directory to be deleted</param>
        public void DeleteDir(string remotePath)
        {
            try
            {
                this.MustDeleteDir(remotePath);
            }
            catch (ApiException ex)
            {
                if (ex.AgileStatusCode == AgileCode.DirNotFound) {
                    return;
                }
                throw ex;
            }
        }

        /// <summary>
        /// deletes a directory
        /// 
        /// errors are thrown
        /// </summary>
        /// <param name="remotePath">the full path to the directory to be deleted</param>
        public void MustDeleteDir(string remotePath)
        {
            this.InvokeCodeBasedMethod("deleteDir", remotePath);
        }
        
        /// <summary>
        /// deletes a file
        /// 
        /// errors are ignored
        /// </summary>
        /// <param name="remotePath">the full path to the file to be deleted</param>
        public void DeleteFile(string remotePath)
        {
            try
            {
                this.MustDeleteFile(remotePath);
            }
            catch (ApiException ex)
            {
                if (ex.AgileStatusCode == AgileCode.FileNotFound)
                {
                    return;
                }
                throw ex;
            }
        }

        /// <summary>
        /// deletes a file
        /// 
        /// errors are thrown
        /// </summary>
        /// <param name="remotePath">the full path to the file to be deleted</param>
        public void MustDeleteFile(string remotePath)
        {
            this.InvokeCodeBasedMethod("deleteFile", remotePath);
        }

        /// <summary>
        /// deletes a file or directory
        /// 
        /// errors are ignored
        /// </summary>
        /// <param name="remotePath">the full path to the file or directory to be deleted</param>
        public void DeleteObject(string remotePath)
        {
            try
            {
                this.MustDeleteObject(remotePath);
            }
            catch (ApiException ex)
            {
                if (ex.AgileStatusCode == AgileCode.DirNotFound)
                {
                    return;
                }
                throw ex;
            }
        }

        /// <summary>
        /// deletes a file or directory
        /// 
        /// errors are thrown
        /// </summary>
        /// <param name="remotePath">the full path to the file or directory to be deleted</param>
        public void MustDeleteObject(string remotePath)
        {
            this.InvokeCodeBasedMethod("deleteObject", remotePath);
        }

        /// <summary>
        /// lists directories with in remotePath
        /// </summary>
        /// <param name="remotePath">full path in which to list directories</param>
        /// <param name="pageOffset">offset from which to start listing items</param>
        /// <param name="pageSize">number of items per page</param>
        /// <param name="includeStat">include stat data </param>
        public ListDirResults ListDir(string remotePath, int pageSize, int pageOffset, bool includeStat)
        {
            try
            {
                var argMaker = new AutoTokenArgMaker(remotePath, pageSize, pageOffset, includeStat);
                var codeGetter = new DictCodeGetter();
                var result = this.rpcRetry.Invoke("listDir", argMaker, codeGetter);

                var results = new ListDirResults();
                results.FromJsonObject(result);
                return results;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnknownApiException(string.Format("listDir failed with remotePath={0}, pageOffset={1}, pageSize={2}",
                    remotePath, pageOffset, pageSize), ex);
            }
        }

        /// <summary>
        /// lists files with in remotePath
        /// </summary>
        /// <param name="remotePath">full path in which to list files</param>
        /// <param name="pageOffset">offset from which to start listing items</param>
        /// <param name="pageSize">number of items per page</param>
        /// <param name="includeStat">include stat data </param>
        public ListFileResults ListFile(string remotePath, int pageSize, int pageOffset, bool includeStat)
        {
            try
            {
                var argMaker = new AutoTokenArgMaker(remotePath, pageSize, pageOffset, includeStat);
                var codeGetter = new DictCodeGetter();
                var result = this.rpcRetry.Invoke("listFile", argMaker, codeGetter);
                
                var results = new ListFileResults();
                results.FromJsonObject(result);
                return results;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnknownApiException(string.Format("listFile failed with remotePath={0}, pageOffset={1}, pageSize={2}",
                    remotePath, pageOffset, pageSize), ex);

            }
        }

        /// <summary>
        /// lists files and directories with in remotePath
        /// </summary>
        /// <param name="remotePath">full path in which to list files and directories</param>
        /// <param name="pageOffset">offset from which to start listing items</param>
        /// <param name="pageSize">number of items per page</param>
        /// <param name="includeStat">include stat data </param>
        public ListPathResults ListPath(string remotePath, int pageSize, string pageOffset, bool includeStat)
        {
            try
            {
                var argMaker = new AutoTokenArgMaker(remotePath, pageSize, pageOffset, includeStat);
                var codeGetter = new DictCodeGetter();
                var result = this.rpcRetry.Invoke("listPath", argMaker, codeGetter);
                var results = new ListPathResults();
                results.FromJsonObject(result);
                return results;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnknownApiException(string.Format("listPath failed with remotePath={0}, pageOffset={1}, pageSize={2}",
                    remotePath, pageOffset, pageSize), ex);
            }
        }

        /// <summary>
        /// authenticates and returns an authentication token to be used in subsequent calls
        /// </summary
        public LoginResult Login()
        {
            this.LoginManager.ClearLogin();
            return this.LoginManager.Login();
        }
         
        /// <summary>
        /// expires the given token
        /// </summary
        /// <param name="token">authentication token</param>
        public void Logout(string token)
        {
            try
            {
                var argMaker = new RawTokenArgMaker(token);
                var codeGetter = new CodeGetter();
                this.rpcRetry.Invoke("logout", argMaker,codeGetter);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnknownApiException(string.Format("logout failed with token={0}", token), ex);
            }
        }

        /// <summary>
        /// creates a directory
        /// 
        /// <remarks>attempting to create non-existent directories recursively is considered an error</remarks>
        /// </summary>
        /// <param name="remotePath">full path to the directory to be created</param>
        public void MakeDir(string remotePath)
        {
            this.InvokeCodeBasedMethod("makeDir", remotePath);
        }

        /// <summary>
        /// recusively creates directories
        /// </summary>
        /// <param name="remotePath">full path to the directory(s) to be created</param>
        public void MakeDir2(string remotePath)
        {
            this.InvokeCodeBasedMethod("makeDir2", remotePath);
        }

        /// <summary>
        /// uploads a file using a local file as input
        /// </summary>
        /// <param name="localPath">full path to the local file to be uploaded</param>
        /// <param name="remotePath">full path to where the file should be uploaded</param>
        public MakeFileResult MakeFile(string localPath, string remotePath) {
            return this.MakeFile(localPath, remotePath, new Dictionary<string, string>());
        }
        /// <summary>
        /// uploads a file using a local file as input with additional headers 
        /// </summary>
        /// <param name="localPath">full path to the local file to be uploaded</param>
        /// <param name="remotePath">full path to where the file should be uploaded</param>
        /// <param name="headers">dictionary containing name value pairs to be sent as HTTP headers</param>
        public MakeFileResult MakeFile(string localPath, string remotePath, Dictionary<string, string> headers)
        {
            using (var fileStream = File.OpenRead(localPath))
            {
                var localFilename = Path.GetFileName(localPath);
                return this.MakeFile(fileStream, remotePath, headers);
            }
        }
        /// <summary>
        /// uploads a file using a stream as input
        /// </summary>
        /// <param name="dataStream">a seekable stream</param>
        /// <param name="remotePath">full path to where the file should be uploaded</param>
        public MakeFileResult MakeFile(Stream dataStream, string remotePath) {
            return this.MakeFile(dataStream, remotePath, new Dictionary<string, string>());
        }
        /// <summary>
        /// uploads a file using a stream as input with additional headers
        /// </summary>
        /// <param name="dataStream">a seekable stream</param>
        /// <param name="remotePath">full path to where the file should be uploaded</param>
        /// <param name="headers">dictionary containing name value pairs to be sent as HTTP headers</param>
        public MakeFileResult MakeFile(Stream dataStream, string remotePath, Dictionary<string, string> headers)
        {
            if (headers == null)
            {
                headers = new Dictionary<string, string>();
            }

            AddDirectoryAndBasename(remotePath, headers);           

            var respHeaders = this.httpRetry.Invoke(this.urls.PostRaw, dataStream, remotePath, headers);
            var result = new MakeFileResult().FromHttpHeaders(respHeaders);
                
            if (result.Status != 0)
            {
                throw new ApiException(result.Status, HttpCode.Ok, string.Format("makeFile failed with remotePath={0}, headers={1}", remotePath, headers));
            }
            return result;
        }

        /// <summary>
        /// renames a file or directory
        /// </summary>
        /// <param name="fromPath">full path to the source file or directory</param>
        /// <param name="toPath">full path destination file or directory</param>
        public void Rename(string fromPath, string toPath)
        {
            this.InvokeCodeBasedMethod("rename", fromPath, toPath);
        }

        /// <summary>
        /// sets the modified time on a file
        /// </summary>
        /// <param name="remotePath">full path to the file</param>
        /// <param name="mtime">new motified time</param>
        public void SetMTime(string remotePath, int mtime)
        {
            this.InvokeCodeBasedMethod("setMTime", remotePath, mtime);
        }

        /// <summary>
        /// returns stat data for a file or directory
        /// </summary>
        /// <param name="remotePath">full path to the file or directory</param>
        public StatResult Stat(string remotePath)
        {
            try
            {
                var argMaker = new AutoTokenArgMaker(remotePath, true);
                var codeGetter = new DictCodeGetter();
                var result = this.rpcRetry.Invoke("stat", argMaker, codeGetter);

                var statusResult = new StatResult();
                statusResult.FromJsonObject(result);
                return statusResult;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnknownApiException(string.Format("stat failed with remotePath={0}", remotePath), ex);
            }
        }

        /// <summary>
        /// Executes an arbitrary JSON-RPC method
        /// </summary>
        /// <remarks>if the codeGetter returns a non-zero code, the method will be retried</remarks>
        /// <param name="method">method to be executed</param>
        /// <param name="codeGetter">an implementation  of ICodeGetter capable of parsing return codes (e.g. DictCodeGetter or CodeGetter) </param>
        /// <param name="args">arguments to be passed into the method</param>
        /// <returns>JSON-RPC result</returns>
        public object ExecRawJson(string method, ICodeGetter codeGetter, params object[] args)
        {
            try
            {
                var argMaker = new AutoTokenArgMaker(args);
                return this.rpcRetry.Invoke(method, argMaker, codeGetter);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnknownApiException(string.Format("Failed sending {0} {1}", method, args), ex);
            }
        }

        /// <summary>
        /// returns the currently authenticated token
        /// </summary>
        public string GetToken()
        {
            var loginResult = this.LoginManager.GetLoginResult();
            if (loginResult == null)
            {
                return null;
            }
            return loginResult.Token;
        }

        /// <summary>
        /// returns the agile path associated with the authenticated token
        /// </summary>
        public string GetAgilePath()
        {
            var loginResult = this.LoginManager.GetLoginResult();
            if (loginResult == null)
            {
                return null;
            }
            return loginResult.Path;
        }

        private void InvokeCodeBasedMethod(string method, params object[] args)
        {
            var codeGetter = new CodeGetter();
            var oCode = this.ExecRawJson(method, codeGetter, args);
            var code = codeGetter.GetCode(oCode);
            if (code == AgileCode.Success)
            {
                return;
            }
            throw new ApiException(code, HttpCode.Ok,  string.Format("Failed sending {0} {1}", method, args));
        }

        private void AddDirectoryAndBasename(string remotePath, Dictionary<string,string> headers)
        {
            var remoteFile = Path.GetFileName(remotePath);
            var remoteDir = Path.GetDirectoryName(remotePath);
            remoteDir = remoteDir.Replace("\\", "/");

            headers.Add("X-Agile-Directory", remoteDir);
            headers.Add("X-Agile-Basename", remoteFile);
        }

        private void httpTransport_OnProgress(object sender, OnProgressArgs args)
        {
            if (this.OnProgress != null)
            {
                this.OnProgress(this, args);
            }
        }
    }
}
