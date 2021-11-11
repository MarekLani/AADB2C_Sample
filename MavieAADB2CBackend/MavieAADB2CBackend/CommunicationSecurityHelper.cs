using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MavieAADB2CBackend
{
    internal class CommunicationSecurityHelper
    {
        internal static bool ValidateBasicAuthentication(HttpRequestData req)
        {
            var headers = req.Headers;
            if (!headers.TryGetValues("authorization", out var authorization))
                return false;

            //Extract credentials
            string encodedUsernamePassword = authorization.First().Substring("Basic ".Length).Trim();
            Encoding encoding = Encoding.GetEncoding("iso-8859-1");
            string usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));
            var split = usernamePassword.Split(':');
            string username = split[0];
            string password = split[1];

            //Check Credentials
            if (username != Environment.GetEnvironmentVariable("BASIC_AUTH_USERNAME").ToString() || password != Environment.GetEnvironmentVariable("BASIC_AUTH_PASSWORD").ToString())
                return false;

            return true;
        }

    }
}
