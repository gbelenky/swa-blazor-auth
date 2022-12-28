
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
    

namespace Api
{
    public class GetClaimsPrincipalFunction
    {
        private readonly IPrincipalService _principalService;

        public GetClaimsPrincipalFunction(
            IPrincipalService principalService
        )
        {
            _principalService = principalService;
        }

        [FunctionName(nameof(GetPrincipal))]
        public async Task<IActionResult> GetPrincipal(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "roles")] HttpRequest request
        )
        {
            var principal = await _principalService.GetPrincipal(request);
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
