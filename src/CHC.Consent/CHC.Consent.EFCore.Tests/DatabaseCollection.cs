using Xunit;

namespace CHC.Consent.EFCore.Tests
{
    [CollectionDefinition(Name)]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
        public const string Name = "Database";
    }
}