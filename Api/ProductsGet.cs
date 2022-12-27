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
    private ILogger log;

    public ProductsGet(IProductData productData, ILogger log)
    {
        this.productData = productData;
        this.log = log;

    }

    [FunctionName("ProductsGet")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products")] HttpRequest req)
    {
        var products = await productData.GetProducts();
        var principal = Parse(req);
        log.LogDebug($"Principal.  Identity{principal.Identity}, {principal.ToString}");
        return new OkObjectResult(products);
    }

    public ClaimsPrincipal Parse(HttpRequest req)
    {
        var principal = new ClientPrincipal();

        if (req.Headers.TryGetValue("x-ms-client-principal", out var header))
        {
            log.LogDebug($"x-ms-client-principal exists");
            var data = header[0];
            var decoded = Convert.FromBase64String(data);
            var json = Encoding.UTF8.GetString(decoded);
            log.LogDebug($"JSON value: {json}");
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

    private class ClientPrincipal
    {
        public string IdentityProvider { get; set; }
        public string UserId { get; set; }
        public string UserDetails { get; set; }
        public IEnumerable<string> UserRoles { get; set; }
    }
}