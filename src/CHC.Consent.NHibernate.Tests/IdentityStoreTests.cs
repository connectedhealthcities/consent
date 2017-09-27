using System;
using System.Collections;
using System.Linq;
using CHC.Consent.Common.Identity;
using CHC.Consent.Identity.Core;
using CHC.Consent.NHibernate.Identity;
using NHibernate;
using NHibernate.Linq;
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

        public IdentityStoreTests(DatabaseFixture db, ITestOutputHelper output)
        {
            this.db = db;
        }

        public void Dispose()
        {
        }

        [Fact]
        public void CanFindAPersonBySimpleIdentityMatch()
        {
            const string identityExternalId = "chc:CanFindAPersonBySimpleIdentityMatch";

            var identityKindId = SaveIdentityKind(identityExternalId);
            
            MakePersonFrom(new PersistedSimpleIdentity
            {
                IdentityKindId = identityKindId,
                Value = "1234"
            });
            
            var identityStore = new NHibernateIdentityStore(db);

            var found = identityStore.FindPerson(
                new IMatch[]
                {
                    new IdentityMatch(new PersistedSimpleIdentity {IdentityKindId = identityKindId, Value = "1234"})
                });

            
            var expected = found.Identities.Cast<ISimpleIdentity>().Single();
            Assert.Equal("1234", expected.Value);
            Assert.Equal(identityKindId, expected.IdentityKindId);

        }

        private Guid SaveIdentityKind(string identityExternalId)
        {
            return db.AsTransaction(s => (Guid)s.Save(new IdentityKind {ExternalId = identityExternalId}));
        }

        public void MakePersonFrom(params IIdentity[] identities)
        {
            db.AsTransaction(
                s =>
                {
                    var person = new PersistedPerson(identities);
                    s.Save(person);
                });
        }
        
        [Fact]
        public void CanFindAPersonBySimpleIdentityMatchWhenPersonHasMoreThanOneIdentity()
        {
            var identityKind1 = SaveIdentityKind("chc:CanFindAPersonBySimpleIdentityMatchWhenPersonHasMoreThanOneIdentity:1");
            var identityKind2 = SaveIdentityKind("chc:CanFindAPersonBySimpleIdentityMatchWhenPersonHasMoreThanOneIdentity:2");


            MakePersonFrom(
                new PersistedSimpleIdentity {IdentityKindId = identityKind1, Value = "1234"},
                new PersistedSimpleIdentity {IdentityKindId = identityKind2, Value = "97"}
            );
            
            var identityStore = new NHibernateIdentityStore(db);

            var result = identityStore.FindPerson(
                new []
                {
                    new IdentityMatch(new PersistedSimpleIdentity {Value = "1234", IdentityKindId = identityKind1})
                }
            );
            
            Assert.NotNull(result);

            Assert.Equal(2, result.Identities.Count());
            
            Assert.All(result.Identities, _ => Assert.IsType<PersistedSimpleIdentity>(_));

            Assert.Contains(
                result.Identities.Cast<PersistedSimpleIdentity>(),
                expected =>
                    "1234" == expected.Value
                    && expected.IdentityKindId == identityKind1);

            Assert.Contains(
                result.Identities.Cast<PersistedSimpleIdentity>(),
                id =>
                    "97" == id.Value
                    && id.IdentityKindId == identityKind2);
        }

        [Fact]
        public void CorrectlyCreatesPerson()
        {
            const string identityExternalId1 = "chc:CorrectlyUpdatesIdentity:1";
            const string identityExternalId2 = "chc:CorrectlyUpdatesIdentity:2";

            var identityKindId1 = SaveIdentityKind(identityExternalId1);
            var identityKindId2 = SaveIdentityKind(identityExternalId2);

            var identityStore = new NHibernateIdentityStore(db);


            var person = (PersistedPerson)identityStore.CreatePerson(
                new[]
                {
                    new PersistedSimpleIdentity
                    {
                        IdentityKindId = identityKindId1,
                        Value = "theKey"
                    }
                });

            var saved = 
                db.AsTransaction(
                s =>
                {
                    var loaded = s.Get<PersistedPerson>(person.Id);
                    NHibernateUtil.Initialize(loaded.Identities);
                    NHibernateUtil.Initialize(loaded.SubjectIdentifiers);
                    return loaded;
                });

            Assert.NotNull(saved);
            Assert.Single(
                saved.Identities,
                i => i.IdentityKindId == identityKindId1 && ((PersistedSimpleIdentity) i).Value == "theKey");
            Assert.Empty(saved.SubjectIdentifiers);

        }
    }
}