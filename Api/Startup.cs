using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;

[assembly: FunctionsStartup(typeof(Api.Startup))]

namespace Api;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
         
        builder.Services.AddSingleton<IProductData, ProductData>();
        var clientId = Environment.GetEnvironmentVariable("ClientId");
        var clientSecret = Environment.GetEnvironmentVariable("ClientSecret");
        var tenantId = Environment.GetEnvironmentVariable("TenantId"); 
        builder.Services.AddTransient<IAuthenticatedGraphClientFactory>(sp =>
    new AuthenticatedGraphClientFactory(
        // The parameters can be sourced from the Azure AD App Registration
        clientId,
        clientSecret,
        tenantId
    ));

        builder.Services.AddTransient<IPrincipalService, PrincipalService>();
    }
}
