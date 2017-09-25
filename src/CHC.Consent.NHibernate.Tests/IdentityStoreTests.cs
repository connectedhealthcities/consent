using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Import.Match;
using CHC.Consent.NHibernate.Identity;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Util;
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
            LoggerProvider.SetLoggersFactory(new OutputLoggerFactory(output));
        }

        public void Dispose()
        {
            LoggerProvider.SetLoggersFactory(new NoLoggingLoggerFactory());
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
                    new SimpleIdentity
                    {
                        Value = "1234",
                        IdentityKind = new IdentityKind {ExternalId = identityExternalId}
                    },
                }).ToArray();

            Assert.Single((IEnumerable) found);
            var expected = found.Cast<SimpleIdentity>().Single();
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

            var found = identityStore.FindExisitingIdentiesFor(
                new Match[] {new IdentityKindId {Id = identityExternalId1}},
                new[]
                {
                    new SimpleIdentity
                    {
                        Value = "1234",
                        IdentityKind = new IdentityKind {ExternalId = identityExternalId1}
                    },
                }).ToArray();

            Assert.Equal(2, found.Length);
            
            Assert.All(found, _ => Assert.IsType<SimpleIdentity>(_));

            Assert.Contains(
                found.Cast<SimpleIdentity>(),
                expected =>
                    "1234" == expected.Value
                    && expected.IdentityKind.ExternalId == identityExternalId1);

            Assert.Contains(
                found.Cast<SimpleIdentity>(),
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
            var keyIdentity = new SimpleIdentity{ IdentityKind = new IdentityKind { ExternalId = identityExternalId1 } , Value = "the key"  };
            db.AsTransaction(_ => _.Save(new IdentityKind {ExternalId = identityExternalId2}));
            
            identityStore.UpsertIdentity(
                match,
                new[]
                {
                    keyIdentity,
                    new SimpleIdentity{ IdentityKind = new IdentityKind { ExternalId = identityExternalId2}, Value = "Updated" }
                });

            var found = identityStore.FindExisitingIdentiesFor(
                match,
                new [] { keyIdentity }
            );
            
            Assert.Contains(
                found.Cast<SimpleIdentity>(),
                id =>
                    id.Value == "Updated" &&
                    id.IdentityKind.ExternalId == identityExternalId2
                    
            );
        }
    }
}