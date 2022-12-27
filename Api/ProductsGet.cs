using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
namespace Api;

public class ProductsGet
{
    private readonly IProductData productData;

    public ProductsGet(IProductData productData)
    {
        this.productData = productData;
    }

    [FunctionName("ProductsGet")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products")] HttpRequest req, ILogger log)
    {
        var products = await productData.GetProducts();
        var principal = Parse(req, log);
        log.LogInformation($"Principal.  Identity{principal.Identity}, {principal.ToString}");
        var cookiePrincipal = GetClientPrincipal(req, log);
        return new OkObjectResult(products);
    }

    public ClaimsPrincipal Parse(HttpRequest req, ILogger log)
    {
        var principal = new ClientPrincipal();

        if (req.Headers.TryGetValue("x-ms-client-principal", out var header))
        {
            log.LogDebug($"x-ms-client-principal exists");
            var data = header[0];
            var decoded = Convert.FromBase64String(data);
            var json = Encoding.UTF8.GetString(decoded);
            log.LogInformation($"JSON value: {json}");
            principal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        principal.UserRoles = principal.UserRoles?.Except(new string[] { "anonymous" }, StringComparer.CurrentCultureIgnoreCase);

        if (!principal.UserRoles?.Any() ?? true)
        {
            return new ClaimsPrincipal();
        }

        var identity = new ClaimsIdentity(principal.IdentityProvider);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, principal.UserId));
        identity.AddClaim(new Claim(ClaimTypes.Name, principal.UserDetails));
        identity.AddClaims(principal.UserRoles.Select(r => new Claim(ClaimTypes.Role, r)));

        return new ClaimsPrincipal(identity);
    }


    private ClientPrincipal GetClientPrincipal(HttpRequest req, ILogger log)
    {
        var principal = new ClientPrincipal();

        var swaCookie = req.Cookies["StaticWebAppsAuthCookie"];

        if (swaCookie != null)
        {
            log.LogInformation("SWA Cookie Found");
            var decoded = Convert.FromBase64String(swaCookie);
            log.LogInformation("SWA Cookie Decoded to a Byte Array");
            var json = Encoding.UTF8.GetString(decoded);
            log.LogInformation($"SWA Cookie JSON: {json}");
            principal = JsonSerializer.Deserialize<ClientPrincipal>(json);
            log.LogInformation($"SWA Cookie Deserialized");
        }

        return principal;
    }

    public class ClientPrincipal
    {
        public string IdentityProvider { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string UserDetails { get; set; } = null!;
        public IEnumerable<string> UserRoles { get; set; } = null!;
        public IEnumerable<ClientPrincipalClaim>? Claims { get; set; }
    }

    public class ClientPrincipalClaim
    {
        public string Typ { get; set; } = null!;
        public string Val { get; set; } = null!;
    }
}