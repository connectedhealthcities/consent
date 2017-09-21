using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace CHC.Consent.EntityFramework.Tests
{
    // Instantiated by Xunit
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CoreContextFixture
    {
        public CoreContext CreateContext(ITestOutputHelper output)
        {
            var options = new DbContextOptionsBuilder<CoreContext>();
            options.UseSqlite("Data Source=test.db;");

            options.UseLoggerFactory(CreateLoggerFactory(output));

            var coreContext = new CoreContext(options.Options);
            coreContext.Database.EnsureCreated();
            return coreContext;
        }

        private static LoggerFactory CreateLoggerFactory(ITestOutputHelper output)
        {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new TestOutputHelperLoggerProvider(output));
            return loggerFactory;
        }
    }
}