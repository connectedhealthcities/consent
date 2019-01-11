using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace CHC.Consent.Testing.Utils
{
    public class LoggerQueue : ILogger
    {
        private readonly string categoryName;

        /// <inheritdoc />
        public LoggerQueue(string categoryName)
        {
            this.categoryName = categoryName;
        }

        private static readonly ConcurrentQueue<(string, Exception)> Queue = new ConcurrentQueue<(string, Exception)>();
        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Enqueue(categoryName, eventId, state, exception, formatter);
        }

        public static void Enqueue<TState>(string category, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Queue.Enqueue(($"{category} [{eventId}] {formatter(state, exception)}", exception));
        }

        public static void DumpTo(Action<string> output)
        {
            while (Queue.TryDequeue(out var details))
            {
                var (message, exception) = details;
                output(message);
                if (exception != null)
                    output(exception.ToString());
            }
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel) => true;
        
        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state) => NoopDisposable.Instance;
    }
}