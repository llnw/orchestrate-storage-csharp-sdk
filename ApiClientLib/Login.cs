using System;
using Newtonsoft.Json.Linq;

namespace ApiClientLib
{
    public interface ILoginManager
    {
        LoginResult Login();
        void ClearLogin();
        LoginResult GetLoginResult();
    }

    public class LoginManager : ILoginManager
    {
        public LoginResult LoginResult;
        private IJsonTransport transport;
        private string user;
        private string password;

        public LoginManager(string user, string password, IJsonTransport transport)
        {
            this.user = user;
            this.password = password;
            this.transport = transport;
        }

        public LoginResult Login()
        {
            if (this.LoginResult != null)
            {
                return this.LoginResult;
            }

            object result = String.Empty;
            try
            {
                result = this.transport.Invoke("login", this.user, this.password, true);
                this.LoginResult = new LoginResult().FromJsonObject((JArray)result);
                return this.LoginResult;
            }
            catch (Exception ex)
            {
                throw new LoginException(result.ToString(), ex);
            }
        }

        public void ClearLogin()
        {
            this.LoginResult = null;
        }

        public LoginResult GetLoginResult()
        {
            return this.LoginResult;
        }
    }
}
