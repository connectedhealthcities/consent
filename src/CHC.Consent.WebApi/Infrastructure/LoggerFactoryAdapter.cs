using System;
using NHibernate;

namespace CHC.Consent.WebApi.Infrastructure
{
    public class LoggerFactoryAdapter : ILoggerFactory
    {
        private readonly Microsoft.Extensions.Logging.ILoggerFactory msLoggerFactory;

        /// <inheritdoc />
        public LoggerFactoryAdapter(Microsoft.Extensions.Logging.ILoggerFactory msLoggerFactory)
        {
            this.msLoggerFactory = msLoggerFactory;
        }

        /// <inheritdoc />
        public IInternalLogger LoggerFor(string keyName)
        {
            return new LoggerAdapter(msLoggerFactory.CreateLogger(keyName));
        }

        /// <inheritdoc />
        public IInternalLogger LoggerFor(Type type)
        {
            return LoggerFor(type.FullName);
        }
    }
}