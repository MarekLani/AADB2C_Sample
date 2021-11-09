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
    public static class ValidateInvitationCode
    {
        [Function("ValidateInvitationCode")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            //Validate invitation code
            if(await InvitationCodeValidator.ValidateInvitationCodeAsync(req, data))
            {
                return req.CreateResponse(System.Net.HttpStatusCode.OK);
            }
            else
            {
                var res = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await res.WriteAsJsonAsync(new B2CResponse("Your invitation code is invalid. Please try again.", System.Net.HttpStatusCode.Unauthorized));
                //Need to override OK, set automatically by WriteJsonAsAsync
                res.StatusCode = System.Net.HttpStatusCode.Unauthorized;
                return res;
            }
        }
    }
}
