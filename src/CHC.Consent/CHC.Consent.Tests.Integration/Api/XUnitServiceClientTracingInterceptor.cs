using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Rest;
using Xunit.Abstractions;

namespace CHC.Consent.Tests.Api.Client
{
    internal class XUnitServiceClientTracingInterceptor : IServiceClientTracingInterceptor
    {
        public ITestOutputHelper Output { get; set; }

        /// <inheritdoc />
        public XUnitServiceClientTracingInterceptor(ITestOutputHelper output)
        {
            Output = output;
        }

        private void WriteLine(string message, params object[] args)
        {
            if(Output == null) return;
            try
            {
                Output.WriteLine(message, args);
            }
            catch (InvalidOperationException)
            {
            }
        }

        /// <inheritdoc />
        public void Configuration(string source, string name, string value)
        {
            WriteLine("Configuration: source:{0} name:{1} value:{2}", source, name, value);
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
            WriteLine("Information: {0}", message);
        }

        /// <inheritdoc />
        public void ReceiveResponse(string invocationId, HttpResponseMessage response)
        {
            WriteLine("Response: InvocationId:{0} {1}",invocationId, response.AsFormattedString());
        }

        /// <inheritdoc />
        public void SendRequest(string invocationId, HttpRequestMessage request)
        {
            
            WriteLine("Response: InvocationId:{0} {1}", invocationId, request.AsFormattedString());
        }

        /// <inheritdoc />
        public void TraceError(string invocationId, Exception exception)
        {
            WriteLine("Error: InvocationId:{0} {1}", invocationId, exception);
        }
    }
}