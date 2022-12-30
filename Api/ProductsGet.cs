using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Text.Json;
using System.Text;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Api.Auth;

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
        ClaimsPrincipal principal = ClaimsService.Parse(req, log);
  
        if (null != principal)
        {
            foreach (Claim claim in principal.Claims)
            {
                log.LogInformation("CLAIM TYPE: " + claim.Type + "; CLAIM VALUE: " + claim.Value + "</br>");
            }
        }
        
        var products = await productData.GetProducts();

        return new OkObjectResult(products);
    }
}