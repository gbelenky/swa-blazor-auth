using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Security.Claims;

namespace Api
{
    public class RolesHoulGap
    {
        [FunctionName("GetRoles")]
        public async Task<IActionResult> GetRoles(
     [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "rolesHoulGap")]
    HttpRequest req, ILogger log)
        {
            var payload = JsonConvert.DeserializeObject<UserPayload>(await req.ReadAsStringAsync());

            var roles = new List<string>();
            foreach (var claim in payload.claims)
            {
                if (claim.typ == ClaimTypes.Role)
                {
                    roles.Add(claim.val);
                }
            }

            // adding some test claims to the roles list
            roles.Add("test1");
            roles.Add("test2");

            foreach (var role in roles)
            {
                log.LogInformation($"Role: {role}");
            }

            return new OkObjectResult(roles);
        }
    }

    public class UserPayload
    {
        public string identityProvider { get; set; }
        public string userId { get; set; }
        public string userDetails { get; set; }
        public string accessToken { get; set; }
        public List<UserClaims> claims { get; set; } = new();

        public class UserClaims
        {
            public string typ { get; set; }
            public string val { get; set; }
        }
    }
}
