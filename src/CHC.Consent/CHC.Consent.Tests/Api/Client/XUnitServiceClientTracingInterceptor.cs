using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Rest;
using Xunit.Abstractions;

namespace CHC.Consent.Tests.Api.Client
{
    internal class XUnitServiceClientTracingInterceptor : IServiceClientTracingInterceptor
    {
        public ITestOutputHelper Output { get; }

        /// <inheritdoc />
        public XUnitServiceClientTracingInterceptor(ITestOutputHelper output)
        {
            Output = output;
        }

        /// <inheritdoc />
        public void Configuration(string source, string name, string value)
        {
            Output.WriteLine("Configuration: source:{0} name:{1} value:{2}", source, name, value);
        }

        /// <inheritdoc />
        public void EnterMethod(string invocationId, object instance, string method, IDictionary<string, object> parameters)
        {
            
        }

        /// <inheritdoc />
        public void ExitMethod(string invocationId, object returnValue)
        {
            
        }

        /// <inheritdoc />
        public void Information(string message)
        {
            Output.WriteLine("Information: {0}", message);
        }

        /// <inheritdoc />
        public void ReceiveResponse(string invocationId, HttpResponseMessage response)
        {
            Output.WriteLine("Response: InvocationId:{0} {1}",invocationId, response.AsFormattedString());
        }

        /// <inheritdoc />
        public void SendRequest(string invocationId, HttpRequestMessage request)
        {
            
            Output.WriteLine("Response: InvocationId:{0} {1}", invocationId, request.AsFormattedString());
        }

        /// <inheritdoc />
        public void TraceError(string invocationId, Exception exception)
        {
            Output.WriteLine("Error: InvocationId:{0} {1}", invocationId, exception);
        }
    }
}