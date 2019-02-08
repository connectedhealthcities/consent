using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace CHC.Consent.EFCore.Tests
{
    public class DatabaseFixture
    {
        private bool initialised = false;
        private static readonly object Sync = new object();

        private static void Initialise(ITestOutputHelper output)
        {
            var context = CreateContext(output);
            
            context.Database.EnsureDeleted();
            context.Database.Migrate();
        }

        private static ConsentContext CreateContext(ITestOutputHelper output, DbConnection connection) => CreateContext(
            output,
            new DbContextOptionsBuilder<ConsentContext>().UseSqlServer(connection));

        private static ConsentContext CreateContext(ITestOutputHelper output, DbContextOptionsBuilder<ConsentContext> sqlOptions)
        {
            var options = sqlOptions
                .UseLoggerFactory(new XunitLoggerProvider(output))
                .EnableSensitiveDataLogging();
            return new ConsentContext(options.Options);
        }

        private static ConsentContext CreateContext(ITestOutputHelper output) =>
            CreateContext(
                output,
                new DbContextOptionsBuilder<ConsentContext>()
                    .UseSqlServer(
                        $@"Server=(localdb)\MSSqlLocalDB;Integrated Security=true;Initial Catalog=ChCEFTest"));

        private void EnsureInitialised(ITestOutputHelper output)
        {
            if (initialised) return;
            lock (Sync)
            {
                if (initialised) return;
                Initialise(output);
                initialised = true;
            }
        }

        public ConsentContext GetContext(ITestOutputHelper output)
        {
            EnsureInitialised(output);
            return CreateContext(output);
        }
        
        public ConsentContext GetContext(ITestOutputHelper output, DbConnection connection)
        {
            EnsureInitialised(output);
            return CreateContext(output, connection);
        }
    }
}