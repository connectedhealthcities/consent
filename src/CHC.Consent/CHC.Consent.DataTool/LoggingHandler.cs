using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Serilog;
using Serilog.Context;

namespace CHC.Consent.DataTool
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
            using (LogContext.PushProperty("Request", request))
            {
                Logger.Verbose("Start Request", request);

                var response = await base.SendAsync(request, cancellationToken);

                Logger.Verbose("Response {response}", response.AsFormattedString());

                return response;
            }
        }
    }
}