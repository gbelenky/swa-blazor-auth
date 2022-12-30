using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;


namespace Api;

public class ProductsGet
{
    private readonly IProductData productData;
     private readonly ILogger log;

    public ProductsGet(IProductData productData, ILoggerFactory loggerFactory)
    {
        this.productData = productData;
        log = loggerFactory.CreateLogger<ProductsGet>();
    }

    [Function("ProductsGet")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products")] HttpRequestData req)
    {
        var header = req.Headers.Where(h => h.Key.StartsWith("x-ms-client-principal"));

        var data = header.FirstOrDefault();
        if (data.Key == null)
        {
            log.LogInformation("No x-ms-client-principal header found");
        }

        var decoded = System.Convert.FromBase64String(data.Value.First());
        var json = System.Text.ASCIIEncoding.ASCII.GetString(decoded);
        log.LogInformation($"x-ms-client-principal content: {json}");

        var products = await productData.GetProducts();

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new { products = products });

        return response;
    }
}