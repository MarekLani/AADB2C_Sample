using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MavieAADB2CBackend
{
    internal class InvitationCodeValidator
    {

        static List<string> InvitationCodes = new List<string>() { "123", "456" };

        internal static async Task<bool> ValidateInvitationCodeAsync(HttpRequestData req, dynamic jsonData, bool codeAsExtensionAttribute = false)
        {
            //Extract Invitaiton Code attribute
            var invitationCodeAttribute = codeAsExtensionAttribute ? $"extension_{ Environment.GetEnvironmentVariable("B2C_EXTENSIONS_APP_ID")}_InvitationCode" : "invitationCode";
            var inviteCode = jsonData[invitationCodeAttribute].ToString();

            return InvitationCodes.Contains(inviteCode);
        }
    }
}
