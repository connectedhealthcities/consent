using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Microsoft.Rest;

namespace CHC.Consent.Api.Client
{
    public class LoggerServiceClientTracingInterceptor : IServiceClientTracingInterceptor
    {
        private ILogger Logger { get; }
        private LogLevel Level { get; }

        /// <inheritdoc />
        public LoggerServiceClientTracingInterceptor(ILogger logger, LogLevel level)
        {
            Logger = logger;
            Level = level;
        }

        void Log(string message, params object[] args)
        {
            Logger.Log(
                Level,
                0,
                new FormattedLogValues(message, args),
                null,
                MessageFormatter);
        }

        void LogError(Exception exception, string message, params object[] args)
        {
            Logger.Log(
                LogLevel.Error,
                0,
                new FormattedLogValues(message, args),
                exception,
                MessageFormatter);
        }

        private static string MessageFormatter(FormattedLogValues values, Exception exception)
        {
            return values.ToString();
        }


        /// <inheritdoc />
        public void Configuration(string source, string name, string value)
        {
            Log("Configuration: source:{source} name:{name} value:{value}", source, name, value);
        }

        /// <inheritdoc />
        public void EnterMethod(
            string invocationId, object instance, string method, IDictionary<string, object> parameters)
        {
            Log("Enter method: {invocationId} {details} {@parameters}", invocationId, new { instance, method, }, parameters);
        }

        /// <inheritdoc />
        public void ExitMethod(string invocationId, object returnValue)
        {
            Log("Exit method: {invocationId} {returnValue}", invocationId, returnValue);
        }

        /// <inheritdoc />
        public void Information(string message)
        {
            Log("Information: {message}", message);
        }

        /// <inheritdoc />
        public void ReceiveResponse(string invocationId, HttpResponseMessage response)
        {
            Log("Response: InvocationId:{invocationId} {response}", invocationId, response.AsFormattedString());
        }

        /// <inheritdoc />
        public void SendRequest(string invocationId, HttpRequestMessage request)
        {

            Log("Response: InvocationId:{invocationId} {response}", invocationId, request.AsFormattedString());
        }

        /// <inheritdoc />
        public void TraceError(string invocationId, Exception exception)
        {
            LogError(exception, "Error: InvocationId:{invocationId}", invocationId);
        }
    }
}