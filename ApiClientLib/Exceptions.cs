using System;

namespace ApiClientLib
{

    public class JsonException : Exception
    {
        public JsonException(string message) : base(message) { }
    }

    public class ApiException : Exception
    {
        public int AgileStatusCode;
        public int HttpStatusCode;

        public ApiException(int agileStatusCode, int httpStatusCode, string message) : base(message)
        {
            this.AgileStatusCode = agileStatusCode;
            this.HttpStatusCode = httpStatusCode;
        }
        public ApiException(int agileStatusCode, int httpStatusCode, string message, Exception innerEx) : base(message, innerEx)
        {
            this.AgileStatusCode = agileStatusCode;
            this.HttpStatusCode = httpStatusCode;
        }
    }

    public class UnknownApiException : ApiException
    {
        public static int code = AgileCode.UnknownError;
        public static int httpCode = HttpCode.Ok;
        public UnknownApiException(string message) : base(code, httpCode, message) { }
        public UnknownApiException(string message, Exception innerEx) : base(code, httpCode, message, innerEx) { }
    }

    public class UnknownReturnCodeException : UnknownApiException
    {
        public UnknownReturnCodeException(int message) : base(message.ToString()) { }
        public UnknownReturnCodeException(string message) : base(message) { }
    }

    public class LoginException : UnknownApiException
    {
        public LoginException(string message) : base(message) { }
        public LoginException(string message, Exception innerEx) : base(message, innerEx) { }
    }
}
