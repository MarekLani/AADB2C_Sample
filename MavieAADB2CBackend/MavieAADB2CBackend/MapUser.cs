using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MavieAADB2CBackend
{
    public static class MapUser
    {
        [Function("MapUser")]
        public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous,  "post")] HttpRequestData req,
            FunctionContext executionContext)
        {

            //Basic Auth Validation
            if (!CommunicationSecurityHelper.ValidateBasicAuthentication(req))
                return req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            //Do whatever user mapping might be needed
            //E.g. map AAD B2C Object Id to custom User ID in Mavie
            //Note that this code might be run repetitively as we are using joint signup sigin flow

            string message = data.objectId.ToString();
            var logger = executionContext.GetLogger("MapUser");
            logger.LogInformation(message);

            var response = req.CreateResponse(HttpStatusCode.OK);


            return response;
        }
    }
}
