using System;
using McMaster.Extensions.Hosting.CommandLine;
using Serilog;

namespace CHC.Consent.DataTool
{
    /// <summary>
    /// Logs errors to Serilog
    /// </summary>
    public class ExceptionLogger : IUnhandledExceptionHandler
    {
        private ILogger Log { get; }

        /// <inheritdoc />
        public ExceptionLogger(ILogger log)
        {
            Log = log;
        }

        /// <inheritdoc />
        public void HandleException(Exception e)
        {
            Log.Fatal(e, "Unhandled error");
            Serilog.Log.CloseAndFlush();
        }
    }
}