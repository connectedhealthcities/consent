using System;
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
    public class IdentityTests
    {
        private readonly DatabaseFixture db;

        public IdentityTests(ITestOutputHelper output, DatabaseFixture db)
        {
            this.db = db;
            LoggerProvider.SetLoggersFactory(new OutputLoggerFactory(output));
        }

        [Fact]
        public void TestIdentityKindPersistence()
        {
            object id = null;
            db.InTransactionalUnitOfWork(
                session =>
                {
                    id = session.Save(
                        new IdentityKind
                        {
                            Format = IdentityKindFormat.Simple,
                            ExternalId = "chc:external:testing"
                        });
                });

            db.InTransactionalUnitOfWork(
                session =>
                {
                    var identityKind = session.Get<IdentityKind>(id);

                    Assert.NotEqual(Guid.Empty, identityKind.Id);
                    Assert.Equal(IdentityKindFormat.Simple, identityKind.Format);
                    Assert.Equal("chc:external:testing", identityKind.ExternalId);


                });
        }

        [Fact]
        public void TestSimpleIdentityPersistence()
        {
            object id = null;
            var identityKindId = Guid.NewGuid();
            db.InTransactionalUnitOfWork(
                _ =>
                {
                    id = _.Save(
                        new SimpleIdentity
                        {
                            IdentityKindId = identityKindId,
                            Value = "simple value"
                        });


                });

            db.InTransactionalUnitOfWork(
                session =>
                {
                    var simpleIdentity = session.Get<NHibernate.Identity.Identity>(id);
                    
                    Assert.NotEqual(default(Guid), simpleIdentity.Id);
                    Assert.IsType<SimpleIdentity>(simpleIdentity);

                    Assert.Equal(identityKindId, simpleIdentity.IdentityKindId);
                    Assert.Equal("simple value", ((SimpleIdentity) simpleIdentity).Value);
                }
            );
        }
    }
}