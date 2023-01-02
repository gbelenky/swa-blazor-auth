using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Api.Auth;
using Api.Data;


namespace Api;

public class ProductsGet
{
    private readonly ILogger log;

    public ProductsGet(ILoggerFactory loggerFactory)
    {
        log = loggerFactory.CreateLogger("ProductsGet");
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
            var response = req.CreateResponse(HttpStatusCode.Unauthorized);
            await response.WriteStringAsync("No x-ms-client-principal header found");
            return response;
        }
        else
        {
            var decoded = System.Convert.FromBase64String(data.Value.First());
            var json = System.Text.ASCIIEncoding.ASCII.GetString(decoded);
            log.LogInformation($"x-ms-client-principal content: {json}");

            var identity = JsonSerializer.Deserialize<Identity>(json);
            
            string customerRole = identity.userRoles.FirstOrDefault(s => s.Contains("Customer"));
            string customer = customerRole.Split(".").LastOrDefault();
            log.LogInformation($"Customer role: {customer}");
            

            string accessRole = identity.userRoles.FirstOrDefault(s => s.Contains("Item"));
            log.LogInformation($"Access role: {accessRole}");

            var products = await ProductData.GetProducts();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(products);
            return response;
        }

    }
}