using System.Net.Http;
using Microsoft.Rest;

namespace CHC.Consent.Api.Client
{
    public partial class Api
    {
        /// <inheritdoc />
        public Api(HttpClient httpClient, ServiceClientCredentials credentials, bool disposeHttpClient = true) : base(httpClient, disposeHttpClient)
        {
            
            Initialize();
            BaseUri = httpClient.BaseAddress;
            Credentials = credentials;
            Credentials?.InitializeServiceClient(this);
        }
    }
}