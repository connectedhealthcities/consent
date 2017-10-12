using CHC.Consent.Testing.NHibernate;
using Xunit;

namespace CHC.Consent.NHibernate.Tests
{
    [CollectionDefinition(Name)]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
        public const string Name = "Db";
    }
}