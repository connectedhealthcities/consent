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
            store = new IdentityKindStore(Db.SessionAccessor);
        }

        [Fact]
        public void StoresNewIdentityKinds()
        {
            string externalId = "urn:chc:consent:tests:identitstore:newidentity:" + Guid.NewGuid();
            
            Db.InTransactionalUnitOfWork(() => store.AddIdentityKind(externalId));

            IdentityKind stored;
            using (var startSession = Db.StartSession())
            {
                stored = startSession.Query<IdentityKind>().FirstOrDefault(k => k.ExternalId == externalId);
            }
            

            Assert.NotNull(stored);
            Assert.Equal(externalId, stored.ExternalId);
            Assert.NotEqual(Guid.Empty, stored.Id);
        }

        [Fact]
        public void ReturnsNullForNonExistantIdentityKinds()
        {
            var identityKind = Db.InTransactionalUnitOfWork(
                () => store.FindIdentityKindByExternalId(
                    "urn:chc:consent:tests:noidentitykind:rubbish:" + Guid.NewGuid()));

            Assert.Null(identityKind);
        }

        [Fact]
        public void FindsAnIdentityKindForExistingIdentityKind()
        {
            var externalId = "urn:chc:consent:tests:identitstore:find:" + Guid.NewGuid();

            Db.InTransactionalUnitOfWork(_ => _.Save(new IdentityKind {ExternalId = externalId}));

            var identityKind = Db.InTransactionalUnitOfWork(() => store.FindIdentityKindByExternalId(externalId));

            Assert.NotNull(identityKind);
            Assert.Equal(externalId, identityKind.ExternalId);
        }
    }
}
