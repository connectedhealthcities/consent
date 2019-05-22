using System.Linq;
using CHC.Consent.EFCore.Entities;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CHC.Consent.EFCore.Tests.Identity
{
    public class AuthorityEntityTests : DbTests
    {
        /// <inheritdoc />
        public AuthorityEntityTests(ITestOutputHelper outputHelper, DatabaseFixture fixture) : base(outputHelper, fixture)
        {
        }

        [Fact]
        public void CanRoundTripAnAuthorityEntity()
        {
            var entity = new AuthorityEntity("Authority", priority: 1, "authority-1");
            createContext.Set<AuthorityEntity>().Add(entity);
            createContext.SaveChanges();

            var found = readContext.Set<AuthorityEntity>().Find(entity.Id);
            
            found.Should().BeEquivalentTo(entity);
        }
    }
}