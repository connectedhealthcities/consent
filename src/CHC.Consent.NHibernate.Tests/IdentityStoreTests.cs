using System;
using System.Linq;
using CHC.Consent.Identity.Core;
using CHC.Consent.Identity.SimpleIdentity;
using CHC.Consent.NHibernate.Consent;
using CHC.Consent.NHibernate.Identity;
using CHC.Consent.Testing.NHibernate;
using NHibernate;
using Xunit;
using Xunit.Abstractions;

namespace CHC.Consent.NHibernate.Tests
{
    
    
    [Collection(DatabaseCollection.Name)]
    public class IdentityStoreTests : IDisposable
    {
        private class IdentityMatch : IIdentityMatch
        {
            public IIdentity Match { get;  }

            public IdentityMatch(IIdentity match)
            {
                Match = match;
            }
        }
        private readonly DatabaseFixture db;
        private readonly NHibernateIdentityStore identityStore;

        public IdentityStoreTests(DatabaseFixture db, ITestOutputHelper output)
        {
            this.db = db;
            identityStore = new NHibernateIdentityStore(db.SessionAccessor, new NaiveIdentityKindProviderHelper());
            LoggerProvider.SetLoggersFactory(new OutputLoggerFactory(output));
        }

        public void Dispose()
        {
        }

        [Fact]
        public void CanFindAPersonBySimpleIdentityMatch()
        {
            const string identityExternalId = "chc:CanFindAPersonBySimpleIdentityMatch";

            Guid identityKindId = db.InTransactionalUnitOfWork(
                () =>
                {
                    var savedIdentityKindId = SaveIdentityKind(identityExternalId);

                    MakePersonFrom(
                        new SimpleIdentity
                        {
                            IdentityKindId = savedIdentityKindId,
                            Value = "1234"
                        });

                    return savedIdentityKindId;
                });

            var found = db.InTransactionalUnitOfWork(
                () => identityStore.FindPerson(
                    new IMatch[]
                    {
                        new IdentityMatch(new SimpleIdentity {IdentityKindId = identityKindId, Value = "1234"})
                    }));

            
            var expected = found.Identities.Cast<ISimpleIdentity>().Single();
            Assert.Equal("1234", expected.Value);
            Assert.Equal(identityKindId, expected.IdentityKindId);

        }

        private Guid SaveIdentityKind(string identityExternalId)
        {
            return (Guid)db.SessionAccessor().Save(new IdentityKind {ExternalId = identityExternalId});
        }

        protected void MakePersonFrom(params NHibernate.Identity.Identity[] identities)
        {
            var person = new Person(identities);
            db.SessionAccessor().Save(person);
        }
        
        [Fact]
        public void CanFindAPersonBySimpleIdentityMatchWhenPersonHasMoreThanOneIdentity()
        {
            var (identityKind1, identityKind2) =
                db.InTransactionalUnitOfWork(
                    () =>
                    {
                        var savedIdentityKind1 = SaveIdentityKind(
                            "chc:CanFindAPersonBySimpleIdentityMatchWhenPersonHasMoreThanOneIdentity:1");
                        var savedIdentityKind2 = SaveIdentityKind(
                            "chc:CanFindAPersonBySimpleIdentityMatchWhenPersonHasMoreThanOneIdentity:2");


                        MakePersonFrom(
                            new SimpleIdentity {IdentityKindId = savedIdentityKind1, Value = "1234"},
                            new SimpleIdentity {IdentityKindId = savedIdentityKind2, Value = "97"}
                        );

                        return (identity1:savedIdentityKind1, identity2:savedIdentityKind2);
                    });

            var result = db.InTransactionalUnitOfWork(
                () => identityStore.FindPerson(
                    new[]
                    {
                        new IdentityMatch(new SimpleIdentity {Value = "1234", IdentityKindId = identityKind1})
                    }
                ));
            
            Assert.NotNull(result);

            Assert.Equal(2, result.Identities.Count());
            
            Assert.All(result.Identities, _ => Assert.IsType<SimpleIdentity>(_));

            Assert.Contains(
                result.Identities.Cast<SimpleIdentity>(),
                expected =>
                    "1234" == expected.Value
                    && expected.IdentityKindId == identityKind1);

            Assert.Contains(
                result.Identities.Cast<SimpleIdentity>(),
                id =>
                    "97" == id.Value
                    && id.IdentityKindId == identityKind2);
        }

        [Fact]
        public void CorrectlyCreatesPerson()
        {
            const string identityExternalId1 = "chc:CorrectlyUpdatesIdentity:1";
            const string identityExternalId2 = "chc:CorrectlyUpdatesIdentity:2";


            var identityKindId1 = db.InTransactionalUnitOfWork(
                () =>
                {
                    SaveIdentityKind(identityExternalId2);
                    return SaveIdentityKind(identityExternalId1);
                });


            var person = db.InTransactionalUnitOfWork(() =>  identityStore.CreatePerson(
                new[]
                {
                    new SimpleIdentity
                    {
                        IdentityKindId = identityKindId1,
                        Value = "theKey"
                    }
                }));

            var saved = 
                db.InTransactionalUnitOfWork(
                s =>
                {
                    var loaded = s.Get<Person>(person.Id);
                    NHibernateUtil.Initialize(loaded.Identities);
                    NHibernateUtil.Initialize(loaded.SubjectIdentifiers);
                    return loaded;
                });

            Assert.NotNull(saved);
            Assert.Single(
                saved.Identities,
                i => i.IdentityKindId == identityKindId1 && ((SimpleIdentity) i).Value == "theKey");
            Assert.Empty(saved.SubjectIdentifiers);

        }
    }
}