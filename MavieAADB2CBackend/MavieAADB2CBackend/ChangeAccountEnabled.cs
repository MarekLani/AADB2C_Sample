using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Newtonsoft.Json;

namespace MavieAADB2CBackend
{
    public static class ChangeAccountEnabled
    {
        [Function("ChangeAccountEnabled")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = null)] HttpRequestData req,
            ILogger log)
        {
            // The app registration should be configured to require access to permissions
            // sufficient for the Microsoft Graph API calls the app will be making, and
            // those permissions should be granted by a tenant administrator.
            var scopes = new string[] { "https://graph.microsoft.com/.default" };

            ClientSecretCredential clientSecretCredential = new ClientSecretCredential(
                Environment.GetEnvironmentVariable("AAD_TENANT_ID"), 
                Environment.GetEnvironmentVariable("AAD_CLIENT_ID"), 
                Environment.GetEnvironmentVariable("AAD_CLIENT_SECRET"));

            GraphServiceClient graphServiceClient = new GraphServiceClient(clientSecretCredential, scopes);         

            try
            {

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);

                //1 . find user
                var foundUser = await graphServiceClient.Users[$"{data.userId}"]
                    .Request()
                    .Select("")
                    .GetAsync();

                //2. update user AccountEnabled attribute
                var user = new User
                {
                    AccountEnabled = (bool)data.accountEnabled,
                    
                };

                await graphServiceClient.Users[foundUser.Id].Request().UpdateAsync(user);
            }
            catch(Exception)
            {
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }

            var okRes = req.CreateResponse(HttpStatusCode.OK);
            return okRes;
        }
    }
}
