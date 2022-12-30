using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Collections.Generic;

namespace Api.Auth
{
    public static class RolesEndpoint
    {
        [FunctionName("RolesEndpoint")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "auth-roles")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("auth-roles endpoint called");
            var payload = JsonConvert.DeserializeObject<UserPayload>(await req.ReadAsStringAsync());

            var roles = new List<string>();
            foreach (var claim in payload.claims)
            {
                if (claim.typ == ClaimTypes.Role)
                {
                    roles.Add(claim.val);
                }
            }

            return new OkObjectResult(roles);
            
        }
    }
}
