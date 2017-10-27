using System;
using Microsoft.Extensions.Logging;
using NHibernate;

namespace CHC.Consent.WebApi.Infrastructure
{
    public class LoggerAdapter : IInternalLogger
    {
        public ILogger Logger { get; }

        public LoggerAdapter(ILogger logger)
        {
            Logger = logger;
        }

        /// <inheritdoc />
        public void Error(object message)
        {
            Logger.LogError("{message}", message);
        }

        /// <inheritdoc />
        public void Error(object message, Exception exception)
        {
            Logger.LogError(exception, "{message}", message);
        }

        /// <inheritdoc />
        public void ErrorFormat(string format, params object[] args)
        {
            Logger.LogError(format, args);
        }

        /// <inheritdoc />
        public void Fatal(object message)
        {
            Logger.LogCritical("{message}", message);
        }

        /// <inheritdoc />
        public void Fatal(object message, Exception exception)
        {
            Logger.LogCritical(exception, "{message}", message);
        }
        
        public void Info(object message)
        {
            Logger.LogInformation("{message}", message);
        }

        /// <inheritdoc />
        public void Info(object message, Exception exception)
        {
            Logger.LogInformation(exception, "{message}", message);
        }

        /// <inheritdoc />
        public void InfoFormat(string format, params object[] args)
        {
            Logger.LogInformation(format, args);
        }

        public void Debug(object message)
        {
            Logger.LogDebug("{message}", message);
        }

        /// <inheritdoc />
        public void Debug(object message, Exception exception)
        {
            Logger.LogDebug(exception, "{message}", message);
        }

        /// <inheritdoc />
        public void DebugFormat(string format, params object[] args)
        {
            Logger.LogDebug(format, args);
        }

        public void Warn(object message)
        {
            Logger.LogWarning("{message}", message);
        }

        /// <inheritdoc />
        public void Warn(object message, Exception exception)
        {
            Logger.LogWarning(exception, "{message}", message);
        }

        /// <inheritdoc />
        public void WarnFormat(string format, params object[] args)
        {
            Logger.LogWarning(format, args);
        }

        /// <inheritdoc />
        public bool IsErrorEnabled => Logger.IsEnabled(LogLevel.Error);

        /// <inheritdoc />
        public bool IsFatalEnabled => Logger.IsEnabled(LogLevel.Critical);

        /// <inheritdoc />
        public bool IsDebugEnabled => Logger.IsEnabled(LogLevel.Debug);

        /// <inheritdoc />
        public bool IsInfoEnabled => Logger.IsEnabled(LogLevel.Information);

        /// <inheritdoc />
        public bool IsWarnEnabled => Logger.IsEnabled(LogLevel.Warning);
    }
}