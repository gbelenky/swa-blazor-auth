using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Api.Auth;
using Api.Data;
using Microsoft.Azure.Cosmos;

namespace Api;






public class ProductsGet
{
    private readonly ILogger log;

    private static Lazy<CosmosClient> lazyClient = new Lazy<CosmosClient>(InitializeCosmosClient);
    private static CosmosClient cosmosClient => lazyClient.Value;

    private static CosmosClient InitializeCosmosClient()
    {
        // Perform any initialization here
        var uri = Environment.GetEnvironmentVariable("COSMOS_ACCOUNT");
        var authKey = Environment.GetEnvironmentVariable("COSMOS_KEY");

        return new CosmosClient(uri, authKey);
    }

    public ProductsGet(ILoggerFactory loggerFactory)
    {
        log = loggerFactory.CreateLogger("ProductsGet");
        CosmosClient cosmosClient = InitializeCosmosClient();
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

            //var products = await ProductData.GetProducts();
            var products = await GetProducts(cosmosClient, customer);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(products);
            return response;
        }

    }

    private async Task<IEnumerable<Product>> GetProducts(CosmosClient cosmosClient, string? customer)
    {

        Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(id: "swa-blazor-auth-db");
        // New instance of Container class referencing the server-side container
        Container container = await database.CreateContainerIfNotExistsAsync(
            id: "products",
            partitionKeyPath: "/id",
            throughput: 400
        );

        string queryText = $"SELECT c.Id, c.Name, c.Description, c.Quantity FROM c where c.Customer='{customer}'";
        using FeedIterator<Product> feed = container.GetItemQueryIterator<Product>(queryText);

        // Iterate query result pages
        while (feed.HasMoreResults)
        {
            FeedResponse<Product> response = await feed.ReadNextAsync();

            // Iterate query results
            foreach (Product item in response)
            {
                Console.WriteLine($"Found item:\t{item.Name}");
            }
            return response;
        }

        return null;
    }
}