using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Collections.Generic;
using System.Dynamic;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Linq;
using Newtonsoft.Json;

namespace MavieAADB2CBackend
{
    public static class ValidateUserRegistration
    {

        static List<string> InvitationCodes = new List<string>() { "123", "456" };

        [Function("ValidateUserRegistration")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req,
            ILogger log)
        {
            //Basic Auth Validation
            if(!CommunicationSecurityHelper.ValidateBasicAuthentication(req))
                return req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            //Validate invitation code
            if(!(await InvitationCodeValidator.ValidateInvitationCodeAsync(req, data, codeAsExtensionAttribute : true)))
            {
                var res = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await res.WriteAsJsonAsync(new B2CResponse("Your invitation code is invalid. Please try again.", System.Net.HttpStatusCode.Unauthorized));
                //Need to override OK, set automatically by WriteJsonAsAsync
                res.StatusCode = System.Net.HttpStatusCode.Unauthorized;
                return res;
            }

            //Extract marketing consent
            //string marketingConsentAttribute = $"extension_{Environment.GetEnvironmentVariable("B2C_EXTENSIONS_APP_ID") }_MarketingConsent";
            //var marketingConsent = (bool)data[marketingConsentAttribute];

            dynamic response = new ExpandoObject();
            IDictionary<string, object> underlyingResponseObject = response;

            var b2cResponse = new B2CResponse("", System.Net.HttpStatusCode.OK);

            foreach (var property in b2cResponse.GetType().GetProperties())
                underlyingResponseObject.Add(property.Name, property.GetValue(b2cResponse));

            //If it is desired to not to store the activation code
            //underlyingResponseObject.Add(invitationCodeAttribute, ""); //overwrites extension attribute to "" in order to not store it in the directory

            //SET Organization
            string organizationAtribute = $"extension_{Environment.GetEnvironmentVariable("B2C_EXTENSIONS_APP_ID") }_Organization";
            underlyingResponseObject.Add(organizationAtribute, "SomeOrg");

            //Construct Display Name
            underlyingResponseObject.Add("displayName", $"{data["givenName"]} {data["surname"]}");

            var okRes = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await okRes.WriteAsJsonAsync(underlyingResponseObject);
            return okRes;
        }
    }
}
