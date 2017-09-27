using System;
using System.Collections;
using System.Linq;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Import.Match;
using CHC.Consent.NHibernate.Identity;
using Xunit;
using Xunit.Abstractions;

namespace CHC.Consent.NHibernate.Tests
{
    [Collection(DatabaseCollection.Name)]
    public class IdentityStoreTests : IDisposable
    {
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
            
            MakePersonFrom(new PersistedSimpleIdentity
            {
                IdentityKind = new IdentityKind{ExternalId = identityExternalId},
                Value = "1234"
            });
            
            var identityStore = new NHibernateIdentityStore(db);

            var found = identityStore.FindExisitingIdentiesFor(
                new Match[] {new IdentityKindId {Id = identityExternalId}},
                new[]
                {
                    new PersistedSimpleIdentity
                    {
                        Value = "1234",
                        IdentityKind = new IdentityKind {ExternalId = identityExternalId}
                    },
                }).ToArray();

            Assert.Single((IEnumerable) found);
            var expected = found.Cast<ISimpleIdentity>().Single();
            Assert.Equal("1234", expected.Value);
            Assert.Equal(identityExternalId, expected.IdentityKind.ExternalId);

        }

        public void MakePersonFrom(params PersistedIdentity[] identities)
        {
            db.AsTransaction(
                s =>
                {
                    foreach (var identityKind in identities.Select(_ => _.IdentityKind).Distinct())
                    {
                        s.SaveOrUpdate(identityKind);
                    }

                    var person = new PersistedPerson();

                    foreach (var persistedIdentity in identities)
                    {
                        person.Identities.Add(persistedIdentity);
                        persistedIdentity.Person = person;
                    }

                    s.Save(person);
                });
        }
        
        [Fact]
        public void CanFindAPersonBySimpleIdentityMatchWhenPersonHasMoreThanOneIdentity()
        {
            const string identityExternalId1 = "chc:CanFindAPersonBySimpleIdentityMatchWhenPersonHasMoreThanOneIdentity:1";
            const string identityExternalId2 = "chc:CanFindAPersonBySimpleIdentityMatchWhenPersonHasMoreThanOneIdentity:2";
            
            var identityKind1 = new IdentityKind {ExternalId = identityExternalId1};
            var identityKind2 = new IdentityKind {ExternalId = identityExternalId2};


            MakePersonFrom(
                new PersistedSimpleIdentity {IdentityKind = identityKind1, Value = "1234"},
                new PersistedSimpleIdentity {IdentityKind = identityKind2, Value = "97"}
            );
            
            var identityStore = new NHibernateIdentityStore(db);

            var result = identityStore.FindExisitingIdentiesFor(
                new Match[] {new IdentityKindId {Id = identityExternalId1}},
                new[]
                {
                    new PersistedSimpleIdentity {IdentityKind = new IdentityKind {ExternalId = identityExternalId1}, Value = "1234"}

                });
            
            Assert.NotNull(result);

            var found = result.ToArray();
            

            Assert.Equal(2, found.Length);
            
            Assert.All(found, _ => Assert.IsType<PersistedSimpleIdentity>(_));

            Assert.Contains(
                found.Cast<PersistedSimpleIdentity>(),
                expected =>
                    "1234" == expected.Value
                    && expected.IdentityKind.ExternalId == identityExternalId1);

            Assert.Contains(
                found.Cast<PersistedSimpleIdentity>(),
                id =>
                    "97" == id.Value
                    && id.IdentityKind.ExternalId == identityExternalId2);
        }

        [Fact]
        public void CorrectlyUpdatesIdentity()
        {
            const string identityExternalId1 = "chc:CorrectlyUpdatesIdentity:1";
            const string identityExternalId2 = "chc:CorrectlyUpdatesIdentity:2";
            MakePersonFrom(new PersistedSimpleIdentity{ IdentityKind = new IdentityKind { ExternalId = identityExternalId1 }, Value = "the key"});
            
            var identityStore = new NHibernateIdentityStore(db);

            var match = new Match[] {new IdentityKindId {Id = identityExternalId1}};
            var keyIdentity = new PersistedSimpleIdentity { IdentityKind = new IdentityKind { ExternalId = identityExternalId1 } , Value = "the key"  };
            db.AsTransaction(_ => _.Save(new IdentityKind {ExternalId = identityExternalId2}));
            
            identityStore.UpsertIdentity(
                match,
                new[]
                {
                    keyIdentity,
                    new PersistedSimpleIdentity{ IdentityKind = new IdentityKind { ExternalId = identityExternalId2}, Value = "Updated" }
                });

            var found = identityStore.FindExisitingIdentiesFor(
                match,
                new [] { keyIdentity }
            );
            
            Assert.Contains(
                found.Cast<PersistedSimpleIdentity>(),
                id =>
                    id.Value == "Updated" &&
                    id.IdentityKind.ExternalId == identityExternalId2
                    
            );
        }
    }
}