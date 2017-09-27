using System;
using System.Linq;
using CHC.Consent.Identity.Core;
using CHC.Consent.NHibernate.Identity;
using NHibernate.Linq;
using Xunit;

namespace CHC.Consent.NHibernate.Tests
{
    [Collection(DatabaseCollection.Name)]
    public class PersonTest
    {
        private class SimpleIdentity : ISimpleIdentity
        {            
            public Guid IdentityKindId { get; set; }
            public string Value { get; set; }
        }

        private readonly DatabaseFixture db;

        public PersonTest(DatabaseFixture db)
        {
            this.db = db;
        }

        [Fact]
        public void ANewPersonHasIdentities()
        {
            var person = new PersistedPerson(
                new[] {new SimpleIdentity {IdentityKindId = Guid.NewGuid(), Value = "Test"},});
            
            Assert.NotEmpty(person.Identities);
            Assert.Single(
                person.Identities,
                _ => _.IdentityKindId != Guid.Empty && ((PersistedSimpleIdentity) _).Value == "Test");
        }

        [Fact]
        public void SavingAPersonSavesIdentities()
        {
            var personId = (Guid)db.AsTransaction(
                s =>
                    s.Save(
                        new PersistedPerson(
                            new[] {new SimpleIdentity {IdentityKindId = Guid.NewGuid(), Value = "Test Save"}})));

            var identities = db.AsTransaction(
                s => s.Query<PersistedIdentity>().Where(_ => _.Person.Id == personId).ToArray());

            Assert.Single(
                identities,
                _ => _.IdentityKindId != Guid.Empty && ((PersistedSimpleIdentity) _).Value == "Test Save");

        }
    }
}