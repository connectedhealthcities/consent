using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace CHC.Consent.Testing.Utils
{
    public class XunitLogger<T> : XunitLogger, ILogger<T>
    {
        /// <inheritdoc />
        public XunitLogger(ITestOutputHelper testOutputHelper, string categoryName) : base(testOutputHelper, categoryName)
        {
        }
    }
}