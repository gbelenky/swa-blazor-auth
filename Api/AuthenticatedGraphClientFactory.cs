using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace Api
{
    public interface IAuthenticatedGraphClientFactory
    {
        (GraphServiceClient, string) GetAuthenticatedGraphClientAndClientId();
    }

    public class AuthenticatedGraphClientFactory : IAuthenticatedGraphClientFactory
    {
        private GraphServiceClient? _graphServiceClient;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _tenantId;

        public AuthenticatedGraphClientFactory(
            string clientId,
            string clientSecret,
            string tenantId
        )
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _tenantId = tenantId;
        }

        public (GraphServiceClient, string) GetAuthenticatedGraphClientAndClientId()
        {
            var authenticationProvider = CreateAuthenticationProvider();

            _graphServiceClient = new GraphServiceClient(authenticationProvider);

            return (_graphServiceClient, _clientId);
        }

        private IAuthenticationProvider CreateAuthenticationProvider()
        {
            // this specific scope means that application will default to what is defined in the application registration rather than using dynamic scopes
            string[] scopes = new string[]
            {
                "https://graph.microsoft.com/.default"
            };

            var confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(_clientId)
                .WithAuthority($"https://login.microsoftonline.com/{_tenantId}/v2.0")
                .WithClientSecret(_clientSecret)
                .Build();

            return new MsalAuthenticationProvider(confidentialClientApplication, scopes); ;
        }
    }

    public class MsalAuthenticationProvider : IAuthenticationProvider
    {
        private readonly IConfidentialClientApplication _clientApplication;
        private readonly string[] _scopes;

        public MsalAuthenticationProvider(IConfidentialClientApplication clientApplication, string[] scopes)
        {
            _clientApplication = clientApplication;
            _scopes = scopes;
        }

        /// <summary>
        /// Update HttpRequestMessage with credentials
        /// </summary>
        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            var token = await GetTokenAsync();

            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
        }

        /// <summary>
        /// Acquire Token
        /// </summary>
        public async Task<string?> GetTokenAsync()
        {
            var authResult = await _clientApplication.AcquireTokenForClient(_scopes).ExecuteAsync();

            return authResult.AccessToken;
        }
    }
}