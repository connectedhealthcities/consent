using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Import.Match;
using CHC.Consent.NHibernate.Identity;
using NHibernate;
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
        }

        public void Dispose()
        {
        }

        [Fact]
        public void CanFindAPersonBySimpleIdentityMatch()
        {
            const string identityExternalId = "chc:CanFindAPersonBySimpleIdentityMatch";
            db.AsTransaction(
                s =>
                {
                    
                    var identityKind = new IdentityKind {ExternalId = identityExternalId};
                    s.Save(identityKind);
                    var person = new PersistedPerson
                    {
                        Identities = new List<PersistedIdentity>
                        {
                            new PersistedSimpleIdentity {IdentityKind = identityKind, Value = "1234"}
                        }
                    };
                    foreach (var identity in person.Identities)
                    {
                        identity.Person = person;
                    }
                    s.Save(person);
                }
            );

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
        
        [Fact]
        public void CanFindAPersonBySimpleIdentityMatchWhenPersonHasMoreThanOneIdentity()
        {
            const string identityExternalId1 = "chc:CanFindAPersonBySimpleIdentityMatchWhenPersonHasMoreThanOneIdentity:1";
            const string identityExternalId2 = "chc:CanFindAPersonBySimpleIdentityMatchWhenPersonHasMoreThanOneIdentity:2";
            db.AsTransaction(
                s =>
                {
                    
                    var identityKind1 = new IdentityKind {ExternalId = identityExternalId1};
                    var identityKind2 = new IdentityKind {ExternalId = identityExternalId2};
                    s.Save(identityKind1);
                    s.Save(identityKind2);
                    var person = new PersistedPerson
                    {
                        Identities = new List<PersistedIdentity>
                        {
                            new PersistedSimpleIdentity {IdentityKind = identityKind1, Value = "1234"},
                            new PersistedSimpleIdentity { IdentityKind = identityKind2, Value = "97"}
                        }
                    };
                    foreach (var identity in person.Identities)
                    {
                        identity.Person = person;
                    }
                    s.Save(person);
                }
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
                expected =>
                    "97" == expected.Value
                    && expected.IdentityKind.ExternalId == identityExternalId2);
        }
    }
}