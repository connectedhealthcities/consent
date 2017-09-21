using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace CHC.Consent.EntityFramework.Tests
{
    /// <summary>
    /// Provides an <see cref="ILogger"/> wrapper around xunit's <see cref="ITestOutputHelper"/> so that 
    /// logging output can be seen in the test output
    /// </summary>
    class TestOutputHelperLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper output;

        public TestOutputHelperLoggerProvider(ITestOutputHelper output)
        {
            this.output = output;
        }

        public void Dispose()
        {
                
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new TestOutputHelperLogger(output);
        }

        private class TestOutputHelperLogger : ILogger
        {
            private readonly ITestOutputHelper output;

            public TestOutputHelperLogger(ITestOutputHelper output)
            {
                this.output = output;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                output.WriteLine(formatter(state,exception));
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return new PeteScope();
            }

            private class PeteScope : IDisposable
            {
                public void Dispose()
                {
                }
            }
        }
    }
}