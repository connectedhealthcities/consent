using System;
using System.Configuration;
using System.Xml.Linq;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Utils;
using NHibernate;
using Xunit;
using Xunit.Abstractions;

namespace CHC.Consent.NHibernate.Tests
{
    [Collection(DatabaseCollection.Name)]
    public class IdentityTests : IClassFixture<DatabaseFixture>
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
            db.AsTransaction(
                _ =>
                {
                    var identityKind =
                        new IdentityKind {Format = IdentityKindFormat.Simple, ExternalId = "chc:external:testing"};
                    
                    _.Save(identityKind);

                    id = _.Save(
                        new SimpleIdentity
                        {
                            IdentityKind = identityKind,
                            Value = "simple value"
                        });


                });

            db.AsTransaction(
                session =>
                {
                    var simpleIdentity = session.Get<Identity>(id);
                    NHibernateUtil.Initialize(simpleIdentity.IdentityKind);

                    Assert.NotEqual(default(long), simpleIdentity.Id);
                    Assert.IsType<SimpleIdentity>(simpleIdentity);

                    Assert.Equal("chc:external:testing", simpleIdentity.IdentityKind.ExternalId);
                    Assert.Equal("simple value", ((SimpleIdentity) simpleIdentity).Value);
                }
            );
        }
    }
}