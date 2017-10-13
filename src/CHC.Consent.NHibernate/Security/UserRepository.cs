using System;
using System.Linq;
using CHC.Consent.Security;
using NHibernate;
using NHibernate.Linq;

namespace CHC.Consent.NHibernate.Security
{
    public class UserRepository : IJwtIdentifiedUserRepository
    {
        private Func<ISession> SessionAccessor { get; }

        /// <inheritdoc />
        public UserRepository(Func<ISession> sessionAccessor)
        {
            SessionAccessor = sessionAccessor;
        }

        /// <inheritdoc />
        public IUser FindUserBy(string issuer, string subject)
        {
            return SessionAccessor().Query<User>().SingleOrDefault(
                user =>
                    user.Logins
                        .OfType<JwtLogin>()
                        .Any(id => id.Issuer == issuer && id.Subject == subject));
        }
    }
}