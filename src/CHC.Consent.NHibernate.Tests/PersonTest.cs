using System;
using System.Linq;
using CHC.Consent.Common.Core;
using CHC.Consent.Identity.SimpleIdentity;
using CHC.Consent.NHibernate.Identity;
using CHC.Consent.Testing.NHibernate;
using NHibernate.Linq;
using Xunit;

namespace CHC.Consent.NHibernate.Tests
{
    [Collection(DatabaseCollection.Name)]
    public class PersonTest
    {
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
                new[] {SimpleIdentity(value:"Test"),});
            
            Assert.NotEmpty(person.Identities);
            Assert.Single(
                person.Identities,
                _ => _.IdentityKindId != Guid.Empty && ((PersistedSimpleIdentity) _).Value == "Test");
        }

        [Fact]
        public void SavingAPersonSavesIdentities()
        {
            var personId = (Guid)db.InTransactionalUnitOfWork(
                s =>
                    s.Save(
                        new PersistedPerson(
                            new[] {SimpleIdentity(value:"Test Save")})));

            var identities = db.InTransactionalUnitOfWork(
                s => s.Query<PersistedIdentity>().Where(_ => _.Person.Id == personId).ToArray());

            Assert.Single(
                identities,
                _ => _.IdentityKindId != Guid.Empty && ((PersistedSimpleIdentity) _).Value == "Test Save");

        }

        [Fact]
        public void APersonCanAddNewSubjectIdentifiers()
        {
            var subjectIdentifierIdentity = SimpleIdentity(value:"Subject Identifier Identity");
            var study = new Study {Id = Guid.NewGuid()};
            var person = new PersistedPerson(
                new[]
                {
                    SimpleIdentity(value:"Something"),
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

        private PersistedSimpleIdentity SimpleIdentity(string value)
        {
            return new PersistedSimpleIdentity { Value = value, IdentityKindId = Guid.NewGuid() };
        }
    }
}