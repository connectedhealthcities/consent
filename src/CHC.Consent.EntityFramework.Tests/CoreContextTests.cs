using CHC.Consent.Common.Core;
using Xunit;
using Xunit.Abstractions;

namespace CHC.Consent.EntityFramework.Tests
{
    [Collection(CoreContextCollection.CollectionName)]
    public class CoreContextTests
    {
        private readonly ITestOutputHelper output;
        private readonly CoreContextFixture coreContextFixture;

        public CoreContextTests(ITestOutputHelper output, CoreContextFixture coreContextFixture)
        {
            this.output = output;
            this.coreContextFixture = coreContextFixture;
        }

        [Fact]
        public void CanSaveStudy()
        {
            using (var coreContext = coreContextFixture.CreateContext(output))
            {
                var entity = new Study();
                coreContext.Studies.Add(entity);
                coreContext.SaveChanges();
                Assert.NotEqual(0, entity.Id);
            }   
        }
    }
}