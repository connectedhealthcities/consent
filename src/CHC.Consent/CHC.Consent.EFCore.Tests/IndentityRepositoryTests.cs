using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.EFCore.IdentifierAdapters;
using Xunit;
using Xunit.Abstractions;

namespace CHC.Consent.EFCore.Tests
{
    public class IndentityRepositoryTests : DbTests
    {
        [Fact]
        public void ThisTestIsInTheWrongPlace()
        {
            var personOne = new PersonEntity {NhsNumber = "4333443"};
            var personTwo = new PersonEntity {NhsNumber = "6655666"};

            Context.AddRange(personOne, personTwo);
            Context.SaveChanges();
            
            var registry = new PersonIdentifierRegistry();
            registry.Add<NhsNumberIdentifier, NhsNumberIdentifierAdapter>();

            var storeProvider = (IStoreProvider)new ContextStoreProvider (CreateNewContextInSameTransaction());

            var repository = new IdentityRepository(
                storeProvider.Get<PersonEntity>(), 
                registry,
                storeProvider);

            Assert.Equal(repository.FindPersonBy(new NhsNumberIdentifier(personOne.NhsNumber)), personOne);
            Assert.Equal(repository.FindPersonBy(new NhsNumberIdentifier(personTwo.NhsNumber)), personTwo);
            Assert.Null(repository.FindPersonBy(new NhsNumberIdentifier("7787773")));
        }

        /// <inheritdoc />
        public IndentityRepositoryTests(ITestOutputHelper outputHelper, DatabaseFixture fixture) : base(outputHelper, fixture)
        {
        }
    }
}