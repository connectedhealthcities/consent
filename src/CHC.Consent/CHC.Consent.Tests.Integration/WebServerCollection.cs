using Xunit;

namespace CHC.Consent.Tests
{
    [CollectionDefinition(Name)]
    public class WebServerCollection : ICollectionFixture<WebServerFixture>
    {
        public const string Name = "Web Server";
    }
}