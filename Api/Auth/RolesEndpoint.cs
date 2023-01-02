using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Net;
using System.Security.Claims;


namespace Api.Auth
{
    public class RolesEndpoint
    {
        private readonly ILogger log;
        public RolesEndpoint(ILoggerFactory loggerFactory)
        {
            log = loggerFactory.CreateLogger<RolesEndpoint>();
        }

        [Function("RolesEndpoint")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "auth-roles")] HttpRequestData req)
        {
            log.LogInformation("auth-roles endpoint called");
            var json = await req.ReadAsStringAsync();
            log.LogInformation($"json: {json}");
            var payload = JsonSerializer.Deserialize<UserPayload>(json);
            if (payload == null)
            {
                log.LogInformation("No payload found");
                var response = req.CreateResponse(HttpStatusCode.Unauthorized);
                return response;
            } else  {
                log.LogInformation($"payload with the following number of claims found: {payload.claims.Count}");
            }

            var roles = new List<string>();

            if (payload.claims != null && payload.claims.Count > 0)
            {
                foreach (var claim in payload.claims)
                {
                    if (claim.typ == ClaimTypes.Role)
                    {
                        roles.Add(claim.val);
                        log.LogInformation($"claim found: {claim.typ}, claim value: {claim.val}");
                    }
                }
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(new { roles = roles });
                return response;
            }
            else
            {
                log.LogInformation("No claims found");
                var response = req.CreateResponse(HttpStatusCode.Unauthorized);
                return response;
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
}
