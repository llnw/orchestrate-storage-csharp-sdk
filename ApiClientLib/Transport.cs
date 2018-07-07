using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApiClientLib
{
    public delegate void OnProgressHandler(object sender, OnProgressArgs args);

    public interface IJsonTransport
    {
        object Invoke(string method, params object[] args);
    }

    public class JsonTransport :IJsonTransport
    {
        private int lastId;
        private Uri url;
        private int timeout;

        public JsonTransport(Uri url) : this(url, 30000) {}
        public JsonTransport(Uri url, int timeout)
        {
            this.url = url;
            this.lastId = 1;
            this.timeout = timeout;
        }

        public object Invoke(string method, params object[] args)
        {
            var request = HttpWebRequest.Create(url);
            request.Timeout = this.timeout;
            request.Method = "POST";

            using (var stream = request.GetRequestStream())
            {
                using (var writer = new StreamWriter(stream))
                {
                    JObject call = new JObject(
                        new JProperty("id", ++this.lastId),
                        new JProperty("method", method),
                        new JProperty("params", args)
                    );
                    writer.Write(call.ToString());
                }
            }

            using (var response = request.GetResponse())
            {
                using (var stream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        JObject answer = JObject.Parse(reader.ReadToEnd());
                        var errorObject = answer["error"];
                        if (errorObject.Type != JTokenType.Null)
                        {
                            throw new JsonException(errorObject.ToString());
                        }

                        return answer["result"];
                    }
                }
            }
        }
    }

    public interface IHttpTransport
    {
        WebHeaderCollection Post(Uri apiUrl, string token, Stream dataStream, string tag, Dictionary<string, string> headers);
    }

    public class HttpTransport : IHttpTransport
    {
        int timeout;

        public HttpTransport(int timeout)
        {
            this.timeout = timeout;
        }

        public WebHeaderCollection Post(Uri apiUrl, string token, Stream dataStream, string tag, Dictionary<string, string> headers)
        {
            var request = HttpWebRequest.Create(apiUrl);
            request.Timeout = this.timeout;
            request.Method = "POST";

            request.Headers.Set("X-Agile-Authorization", token);

            foreach (var header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            using (var requestStream = request.GetRequestStream())
            {
                this.CopyStream(dataStream, requestStream, (int)dataStream.Length, tag);
            }

            using (var response = request.GetResponse())
            {
                var status = Convert.ToInt32(response.Headers.Get("X-Agile-Status"));
                if (status != AgileCode.Success)
                {
                    throw new UnknownApiException(string.Format("HTTP POST failed Url={0}, ApiStatus={1}", apiUrl, status));
                }
                return response.Headers;
            }
        }

        private void CopyStream(Stream input, Stream output, long bytesToRead, string remotePath)
        {
            byte[] buffer = new byte[32768];
            long totalRead = 0;
            int toRead = buffer.Length;

            while (totalRead < bytesToRead)
            {
                if (bytesToRead - totalRead < buffer.Length)
                {
                    toRead = (int)(bytesToRead - totalRead);
                }

                int read = input.Read(buffer, 0, toRead);
                totalRead += read;
                if (this.OnProgress != null)
                {
                    this.OnProgress(this, new OnProgressArgs(remotePath, read, totalRead, toRead));
                }

                if (read <= 0) break;
                output.Write(buffer, 0, read);
            }
        }

        public event OnProgressHandler OnProgress;
    }

    public class OnProgressArgs
    {
        public long LastBytesRead { get; private set; }
        public long TotalBytesRead { get; private set; }
        public long TotalBytes { get; private set; }
        public string Tag { get; private set; }
        public OnProgressArgs(string tag, long lastRead, long totalRead, long total)
        {
            this.Tag = tag;
            this.LastBytesRead = lastRead;
            this.TotalBytesRead = totalRead;
            this.TotalBytes = total; ;
        }
    }
}
