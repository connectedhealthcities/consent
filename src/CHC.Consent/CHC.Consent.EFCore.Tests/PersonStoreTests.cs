using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CHC.Consent.Common;
using CHC.Consent.Testing.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Xunit;
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
            _testOutputHelper.WriteLine($"{_categoryName} [{eventId}] {formatter(state, exception)}");
            if (exception != null)
                _testOutputHelper.WriteLine(exception.ToString());
        }

        private class NoopDisposable : IDisposable
        {
            public static IDisposable Instance { get; } = new NoopDisposable();
            public void Dispose() { }
        }
    }


    public class DatabaseFixture
    {
        private bool initialised = false;
        private static readonly object Sync = new object();


        private static readonly string DbFileLocation = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            "CHC.Consent.Tests.mdf");

        private static void Initialise(ITestOutputHelper output)
        {
            var context = CreateContext(output);


            context.Database.EnsureDeleted();
            context.Database.Migrate();
        }

        private static ConsentContext CreateContext(ITestOutputHelper output)
        {
            var options = new DbContextOptionsBuilder<ConsentContext>()
                .UseSqlServer(
                    $@"Server=(localdb)\MSSqlLocalDB;Integrated Security=true;Initial Catalog=ChCEntityFrameworkTest")
                .UseLoggerFactory(new XunitLoggerProvider(output));
            var context = new ConsentContext(options.Options);
            return context;
        }

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
    }
    
    [CollectionDefinition(Name)]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
        public const string Name = "Database";
    }

    [Collection(DatabaseCollection.Name)]
    public class PersonStoreTests : IDisposable
    {
        private readonly IDbContextTransaction transaction;
        private ConsentContext Context { get; }


        /// <inheritdoc />
        public PersonStoreTests(ITestOutputHelper outputHelper, DatabaseFixture fixture)
        {
            Context = fixture.GetContext(outputHelper);
            transaction = Context.Database.BeginTransaction();
        }

        [Fact]
        public void CanSaveAPerson()
        {
            var store = new PersonStore(Context.People);

            var person = store.Add(new Person {DateOfBirth = 3.April(2018)});

            Context.SaveChanges();

            Assert.NotEqual(0, person.Id);
        }

        [Fact]
        public void CanQueryPeople()
        {
            Context.People.AddRange(new []
            {
                new PersonEntity { NhsNumber = "11-11-111"},
                new PersonEntity { NhsNumber = "22-22-222"},
                new PersonEntity { NhsNumber = "33-33-333"},
                new PersonEntity { NhsNumber = "44-44-444"},
                new PersonEntity { NhsNumber = "55-55-555"},
            });
            Context.SaveChanges();

            var store = new PersonStore(Context.People);

            var foundPeople = store.Where(_ => _.NhsNumber == "33-33-333").ToArray();

            var person = Assert.Single(foundPeople);
            Assert.NotNull(person);
            Assert.Equal("33-33-333", person.NhsNumber);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            transaction?.Dispose();
            Context?.Dispose();
            
        }
    }
}