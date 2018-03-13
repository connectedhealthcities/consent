using CHC.Consent.Testing.Utils;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace CHC.Consent.EFCore.Tests
{
    public class XunitLoggerProvider : ILoggerFactory
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public XunitLoggerProvider(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public ILogger CreateLogger(string categoryName) => new XunitLogger(_testOutputHelper, categoryName);

        /// <inheritdoc />
        public void AddProvider(ILoggerProvider provider)
        {
            
        }

        public void Dispose()
        { }
    }
}