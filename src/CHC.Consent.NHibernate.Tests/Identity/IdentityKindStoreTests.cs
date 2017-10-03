using System;
using System.Linq;
using CHC.Consent.Identity.Core;
using CHC.Consent.NHibernate.Identity;
using CHC.Consent.Testing.NHibernate;
using NHibernate.Linq;
using Xunit;

namespace CHC.Consent.NHibernate.Tests.Identity
{
    [Collection(DatabaseCollection.Name)]
    public class IdentityKindStoreTests
    {
        private readonly IdentityKindStore store;
        public DatabaseFixture Db { get; }

        public IdentityKindStoreTests(DatabaseFixture db)
        {
            Db = db;
            store = new IdentityKindStore(Db);
        }

        [Fact]
        public void StoresNewIdentityKinds()
        {
            string externalId = "urn:chc:consent:tests:identitstore:newidentity:" + Guid.NewGuid();
            
            store.AddIdentity(externalId);

            var stored = Db.AsTransaction(_ => _.Query<IdentityKind>().FirstOrDefault(k => k.ExternalId == externalId));

            Assert.NotNull(stored);
            Assert.Equal(externalId, stored.ExternalId);
            Assert.NotEqual(Guid.Empty, stored.Id);
        }

        [Fact]
        public void ReturnsNullForNonExistantIdentityKinds()
        {
            var identityKind = store.FindIdentityKindByExternalId("urn:chc:consent:tests:noidentitykind:rubbish:" + Guid.NewGuid());

            Assert.Null(identityKind);
        }

        [Fact]
        public void FindsAnIdentityKindForExistingIdentityKind()
        {
            var externalId = "urn:chc:consent:tests:identitstore:find:" + Guid.NewGuid();

            Db.AsTransaction(_ => _.Save(new IdentityKind {ExternalId = externalId}));

            var identityKind = store.FindIdentityKindByExternalId(externalId);

            Assert.NotNull(identityKind);
            Assert.Equal(externalId, identityKind.ExternalId);
        }
    }
}
