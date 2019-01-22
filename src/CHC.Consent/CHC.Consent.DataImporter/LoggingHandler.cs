using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;

namespace CHC.Consent.DataImporter
{
    internal class LoggingHandler : DelegatingHandler
    {
        public ILogger Logger { get; }

        public LoggingHandler(ILogger logger, HttpMessageHandler innerHandler=null) : base(innerHandler??new HttpClientHandler())
        {
            Logger = logger;
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Logger.LogTrace("Request {request}", request.AsFormattedString());
            
            var response = await base.SendAsync(request, cancellationToken);

            Logger.LogTrace("Response {response}", response.AsFormattedString());
            
            return response;
        }
    }
}