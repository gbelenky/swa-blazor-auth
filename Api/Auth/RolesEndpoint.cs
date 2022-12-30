using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Net;
using System.Security.Claims;


namespace Api.Auth
{
    public static class RolesEndpoint
    {
        [Function("RolesEndpoint")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "auth-roles")] HttpRequestData req,
            ILogger log)
        {
            log.LogInformation("auth-roles endpoint called");
            var json = await req.ReadAsStringAsync();
            log.LogInformation($"json: {json}");
            var payload = JsonSerializer.Deserialize<UserPayload>(json);

            var roles = new List<string>();
            foreach (var claim in payload.claims)
            {
                if (claim.typ == ClaimTypes.Role)
                {
                    roles.Add(claim.val);
                }
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new { roles = roles });
            return response;

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
}
