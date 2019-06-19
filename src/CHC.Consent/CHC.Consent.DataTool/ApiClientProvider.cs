using System;
using System.Net.Http;
using System.Threading.Tasks;
using CHC.Consent.Api.Client;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.Rest;
using Serilog;
using Serilog.Events;

namespace CHC.Consent.DataTool
{
    public class ApiClientProvider
    {
        private DiscoveryResponse discoveryDocument;
        private ApiConfiguration Configuration { get; }
        private ILogger Logger { get; }

        /// <inheritdoc />
        public ApiClientProvider(ApiConfiguration configuration, ILogger logger)
        {
            Configuration = configuration;
            Logger = logger.ForContext<ApiClientProvider>();
            
            ServiceClientTracing.IsEnabled = true;
            ServiceClientTracing.AddTracingInterceptor(
                new LoggerServiceClientTracingInterceptor(Logger, LogEventLevel.Verbose));
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
            Logger.Debug(
                "Getting OAuth2 access token for client {client} from {tokenEndPoint}",
                Configuration.ClientId,
                tokenEndpoint);
            Logger.Verbose(
                "OAuth2 configuration is are {@configuration}", Configuration );
            
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
            Logger.Debug("Getting oauth2 discovery document from {baseUrl}", Configuration.BaseUrl);
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