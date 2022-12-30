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
using System.Linq;

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
            ClaimsPrincipal principal = ClaimsService.Parse(req,log);
            var identity = principal?.Identity;
            var data = new
            {
                Name = identity?.Name ?? "",
                AuthenticationType = identity?.AuthenticationType ?? "",
                Claims = principal?.Claims.Select(c => new { c.Type, c.Value }),
            };

            return new OkObjectResult(data);
        }
    }
}
