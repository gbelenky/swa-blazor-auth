using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Api;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    /*
    .ConfigureServices(services =>
    {
        services.AddSingleton<IProductData, ProductData>();
    })
    */
    .Build();

host.Run();
