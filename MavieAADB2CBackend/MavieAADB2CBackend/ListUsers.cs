using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace MavieAADB2CBackend
{
    public static class ListUsers
    {
        [Function("ListUsers")]
        public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req,
            FunctionContext executionContext)
        {

            var scopes = new string[] { "https://graph.microsoft.com/.default" };

            ClientSecretCredential clientSecretCredential = new ClientSecretCredential(
                Environment.GetEnvironmentVariable("AAD_TENANT_ID"),
                Environment.GetEnvironmentVariable("AAD_CLIENT_ID"),
                Environment.GetEnvironmentVariable("AAD_CLIENT_SECRET"));

            GraphServiceClient graphServiceClient = new GraphServiceClient(clientSecretCredential, scopes);
         

            string organizationAtribute = $"extension_{Environment.GetEnvironmentVariable("B2C_EXTENSIONS_APP_ID") }_Organization";

            var users = await graphServiceClient.Users
                .Request()
                .Select($"surname,givenName,createdDateTime,{organizationAtribute}")
                .GetAsync();
            //Custom attribute stored under user.AdditionalData property

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(users);

            return response;
        }
    }
}
