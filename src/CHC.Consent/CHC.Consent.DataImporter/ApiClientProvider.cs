using System;
using System.Net.Http;
using System.Threading.Tasks;
using CHC.Consent.Api.Client;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;

namespace CHC.Consent.DataImporter
{
    public class ApiClientProvider
    {
        private DiscoveryResponse discoveryDocument;
        private ApiConfiguration Configuration { get; }
        private ILogger<HttpClient> Logger { get; }

        /// <inheritdoc />
        public ApiClientProvider(ApiConfiguration configuration, ILogger<HttpClient> logger)
        {
            Configuration = configuration;
            Logger = logger;
            
            ServiceClientTracing.IsEnabled = true;
            ServiceClientTracing.AddTracingInterceptor(
                new LoggerServiceClientTracingInterceptor(Logger, LogLevel.Trace));
        }

        public async Task<Api.Client.Api> CreateApiClient()
        {
            var accessToken = await GetAccessToken();
            
            return new Api.Client.Api(
                new Uri(Configuration.BaseUrl),
                new TokenCredentials(accessToken),
                new HttpClientHandler {AllowAutoRedirect = false});
        }

        private async Task<string> GetAccessToken()
        {
            
            var tokenEndpoint = await GetTokenEndpoint();
            Logger.LogDebug(
                "Getting OAuth2 access token for client {client} from {tokenEndPoint}",
                Configuration.ClientId,
                tokenEndpoint);
            Logger.LogTrace(
                "OAuth2 credentials are {clientId}:{clientSecret}",
                Configuration.ClientId,
                Configuration.ClientSecret);
            
            using (var tokenHttpClient = CreateTokenHttpClient())
            {
                var tokenResponse = await tokenHttpClient.RequestClientCredentialsTokenAsync(
                    new ClientCredentialsTokenRequest
                    {
                        Scope = "api",
                        ClientId = Configuration.ClientId,
                        ClientSecret = Configuration.ClientSecret,
                        GrantType = OidcConstants.GrantTypes.ClientCredentials,
                        Address = tokenEndpoint
                    });
                EnsureSuccess(tokenResponse);

                return tokenResponse.AccessToken;
            }
        }

        private async Task<string> GetTokenEndpoint()
        {
            return (await GetDiscoveryDocument()).TokenEndpoint;
        }

        private async Task<DiscoveryResponse> GetDiscoveryDocument()
        {
            return discoveryDocument ?? (discoveryDocument = await GetRemoteDiscoveryDocument());
        }

        private async Task<DiscoveryResponse> GetRemoteDiscoveryDocument()
        {
            Logger.LogDebug("Getting oauth2 discovery document from {baseUrl}", Configuration.BaseUrl);
            using (var httpClient = CreateTokenHttpClient())
            {
                var document = await httpClient.GetDiscoveryDocumentAsync();
                EnsureSuccess(document);
                return document;
            }
        }

        private HttpClient CreateTokenHttpClient() =>
            new HttpClient(new LoggingHandler(Logger))
                {BaseAddress = new Uri(Configuration.BaseUrl)};

        private void EnsureSuccess(dynamic tokenResponse)
        {
            if (!tokenResponse.IsError) return;
            
            throw new Exception(tokenResponse.Error);
        }
    }
}