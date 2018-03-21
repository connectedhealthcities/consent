using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

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
    
    public class XunitLogger : ILogger
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly string _categoryName;

        public XunitLogger(ITestOutputHelper testOutputHelper, string categoryName)
        {
            _testOutputHelper = testOutputHelper;
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state) => NoopDisposable.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            try
            {
                _testOutputHelper.WriteLine($"{_categoryName} [{eventId}] {formatter(state, exception)}");
                if (exception != null)
                    _testOutputHelper.WriteLine(exception.ToString());
            }
            catch (InvalidOperationException)
            {
                LoggerQueue.Enqueue(_categoryName, eventId, state, exception, formatter);
            }
        }
    }

    internal class NoopDisposable : IDisposable
    {
        public static IDisposable Instance { get; } = new NoopDisposable();
        public void Dispose() { }
    }

    public class XunitLogger<T> : XunitLogger, ILogger<T>
    {
        /// <inheritdoc />
        public XunitLogger(ITestOutputHelper testOutputHelper, string categoryName) : base(testOutputHelper, categoryName)
        {
        }
    }
    
    
}