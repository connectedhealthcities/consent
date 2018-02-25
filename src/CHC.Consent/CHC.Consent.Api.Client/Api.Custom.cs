using System.Net.Http;

namespace CHC.Consent.Api.Client
{
    public partial class Api
    {
        /// <inheritdoc />
        public Api(HttpClient httpClient, bool disposeHttpClient = true) : base(httpClient, disposeHttpClient)
        {
            Initialize();
            BaseUri = httpClient.BaseAddress;
            
        }
    }
}