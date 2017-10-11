using System;
using CHC.Consent.NHibernate.Security;
using CHC.Consent.Testing.NHibernate;
using NHibernate;
using Xunit;
using Xunit.Abstractions;

namespace CHC.Consent.NHibernate.Tests.Security
{
    [Collection(DatabaseCollection.Name)]
    public class UserRepositoryTest
    {
        private readonly DatabaseFixture db;

        /// <inheritdoc />
        public UserRepositoryTest(DatabaseFixture db, ITestOutputHelper output)
        {
            this.db = db;
        }

        [Fact]
        public void FindsPersonByJwtIdentity()
        {
            var user = new User();
            var jwtLogin = new JwtLogin("urn:chc:consent:nhibernate:userrepository:test", Guid.NewGuid().ToString());
            user.AddLogin(jwtLogin);
            
            db.AsTransaction(s => { s.Save(user); });

            var found = new UserRepository(db).FindUserBy(jwtLogin.Issuer, jwtLogin.Subject);

            Assert.Equal(user.Id, ((User) found).Id);
            Assert.NotEmpty(user.Logins);
        }
    }
}