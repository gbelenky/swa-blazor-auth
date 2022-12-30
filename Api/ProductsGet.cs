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
        var header = req.Headers["x-ms-client-principal"];
        var data = header.FirstOrDefault();
        if (data == null)
        {
            log.LogInformation("No x-ms-client-principal header found");
        }

        var decoded = System.Convert.FromBase64String(data);
        var json = System.Text.ASCIIEncoding.ASCII.GetString(decoded);
        log.LogInformation($"x-ms-client-principal content: {json}");

        var products = await productData.GetProducts();

        return new OkObjectResult(products);
    }
}