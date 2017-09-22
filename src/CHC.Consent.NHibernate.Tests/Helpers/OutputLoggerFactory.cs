using System;
using NHibernate;
using Xunit.Abstractions;

namespace CHC.Consent.NHibernate.Tests
{
    public class OutputLoggerFactory : ILoggerFactory
    {
        private readonly ITestOutputHelper output;

        public OutputLoggerFactory(ITestOutputHelper output)
        {
            this.output = output;
        }

        public IInternalLogger LoggerFor(string keyName)
        {
            return new OutputLogger(keyName, output);
        }

        public IInternalLogger LoggerFor(Type type)
        {
            return LoggerFor(type.FullName);
        }
    }
}