using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public static class EditUserProfile
    {
        [Function("EditUserProfile")]
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
                    .Select("identities,id")
                    .GetAsync();

                // Need to re-create the existing identities since Graph client rejects existing items in requests
                var identity = foundUser.Identities.Select(o => new ObjectIdentity
                {
                    SignInType = o.SignInType,
                    Issuer = o.Issuer,
                    IssuerAssignedId = o.IssuerAssignedId

                }).FirstOrDefault();

                identity.IssuerAssignedId = data.email.ToString();

                //2. update user AccountEnabled attribute
                var user = new User
                {
                    GivenName = data.givenName.ToString(),
                    Surname = data.surname.ToString(),
                    DisplayName = $"{data.givenName} {data.surname}",
                    Identities = new List<ObjectIdentity>() {identity}
                };

                await graphServiceClient.Users[foundUser.Id].Request().UpdateAsync(user);
            }
            catch (Exception)
            {
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }

            var okRes = req.CreateResponse(HttpStatusCode.OK);
            return okRes;
        }
    }
}
