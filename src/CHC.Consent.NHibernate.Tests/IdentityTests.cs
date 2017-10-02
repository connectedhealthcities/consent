using System;
using System.Configuration;
using System.Xml.Linq;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Utils;
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
        private readonly ITestOutputHelper output;
        private readonly DatabaseFixture db;

        public IdentityTests(ITestOutputHelper output, DatabaseFixture db)
        {
            this.output = output;
            this.db = db;
        }

        [Fact]
        public void TestIdentityKindPersistence()
        {
            object id = null;
            db.AsTransaction(
                session =>
                {
                    id = session.Save(
                        new IdentityKind
                        {
                            Format = IdentityKindFormat.Simple,
                            ExternalId = "chc:external:testing"
                        });
                });

            db.AsTransaction(
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
            db.AsTransaction(
                _ =>
                {
                    id = _.Save(
                        new PersistedSimpleIdentity
                        {
                            IdentityKindId = identityKindId,
                            Value = "simple value"
                        });


                });

            db.AsTransaction(
                session =>
                {
                    var simpleIdentity = session.Get<PersistedIdentity>(id);
                    
                    Assert.NotEqual(default(long), simpleIdentity.Id);
                    Assert.IsType<PersistedSimpleIdentity>(simpleIdentity);

                    Assert.Equal(identityKindId, simpleIdentity.IdentityKindId);
                    Assert.Equal("simple value", ((PersistedSimpleIdentity) simpleIdentity).Value);
                }
            );
        }
    }
}