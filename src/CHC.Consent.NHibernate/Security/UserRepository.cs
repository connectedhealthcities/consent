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
            var session = SessionAccessor();
            var foundUser = session.Query<User>().SingleOrDefault(
                user =>
                    user.Logins
                        .OfType<JwtLogin>()
                        .Any(id => id.Issuer == issuer && id.Subject == subject));

            LoadRoleHierarchy(foundUser);

            return foundUser;
        }

        /// <remarks>
        /// TODO: Replace N+1 hierarchical loading with something better
        /// </remarks>
        private static void LoadRoleHierarchy(SecurityPrincipal foundUser)
        {
            var currentPrincipal = foundUser;
            while (currentPrincipal != null)
            {
                NHibernateUtil.Initialize(currentPrincipal.Role);
                currentPrincipal = currentPrincipal.Role;
            }
        }
    }
}