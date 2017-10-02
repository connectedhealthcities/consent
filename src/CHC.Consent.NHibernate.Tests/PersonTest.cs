using System;
using System.Linq;
using CHC.Consent.Common.Core;
using CHC.Consent.Identity.Core;
using CHC.Consent.NHibernate.Identity;
using CHC.Consent.Testing.NHibernate;
using NHibernate.Linq;
using Xunit;

namespace CHC.Consent.NHibernate.Tests
{
    [Collection(DatabaseCollection.Name)]
    public class PersonTest
    {
        private class SimpleIdentity : ISimpleIdentity
        {            
            public Guid IdentityKindId { get; set; } = Guid.NewGuid();
            public string Value { get; set; }
        }

        private class Study : IStudy
        {
            /// <inheritdoc />
            public Guid Id { get; set; }
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
                new[] {new SimpleIdentity { Value = "Test"},});
            
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
                            new[] {new SimpleIdentity { Value = "Test Save"}})));

            var identities = db.AsTransaction(
                s => s.Query<PersistedIdentity>().Where(_ => _.Person.Id == personId).ToArray());

            Assert.Single(
                identities,
                _ => _.IdentityKindId != Guid.Empty && ((PersistedSimpleIdentity) _).Value == "Test Save");

        }

        [Fact]
        public void APersonCanAddNewSubjectIdentifiers()
        {
            var subjectIdentifierIdentity = new SimpleIdentity {Value = "Subject Identifier Identity"};
            var study = new Study {Id = Guid.NewGuid()};
            var person = new PersistedPerson(
                new[]
                {
                    new SimpleIdentity {Value = "Something"},
                    subjectIdentifierIdentity,
                });


            var addedSubjectIdentifier = person.AddSubjectIdentifier(
                study,
                "Subject Identifier",
                new[] {subjectIdentifierIdentity});


            Assert.Equal(study.Id, addedSubjectIdentifier.StudyId);
            Assert.Equal("Subject Identifier", addedSubjectIdentifier.SubjectIdentifier);
            Assert.Single(
                addedSubjectIdentifier.Identities,
                id => id.IdentityKindId == subjectIdentifierIdentity.IdentityKindId &&
                      ((ISimpleIdentity) id).Value == subjectIdentifierIdentity.Value);
        }
    }
}