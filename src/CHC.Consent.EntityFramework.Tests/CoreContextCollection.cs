using Xunit;

namespace CHC.Consent.EntityFramework.Tests
{
    [CollectionDefinition(CollectionName)]
    public class CoreContextCollection : ICollectionFixture<CoreContextFixture>
    {
        public const string CollectionName = "CoreContext";
    }
}