using System;
using System.Linq;
using CHC.Consent.Security;
using NHibernate;
using NHibernate.Linq;

namespace CHC.Consent.NHibernate.Security
{
    public class UserRepository : IJwtIdentifiedUserRepository
    {
        private Authenticatable currentUser;
        private Func<ISession> SessionAccessor { get; }

        /// <inheritdoc />
        public UserRepository(Func<ISession> sessionAccessor)
        {
            SessionAccessor = sessionAccessor;
        }

        /// <inheritdoc />
        public IAuthenticatable FindUserBy(string issuer, string subject)
        {
            return currentUser ?? LoadJwtUser(issuer, subject);
        }

        private Authenticatable LoadJwtUser(string issuer, string subject)
        {
            var session = SessionAccessor();
            currentUser = session.Query<Authenticatable>()
                .SingleOrDefault(
                    user =>
                        user.Logins
                            .OfType<JwtLogin>()
                            .Any(id => id.Issuer == issuer && id.Subject == subject));

            LoadRoleHierarchy();
            
            return currentUser;
        }

        /// <remarks>
        /// TODO: Replace N+1 hierarchical loading with something better
        /// </remarks>
        private void LoadRoleHierarchy()
        {
            SecurityPrincipal currentPrincipal = currentUser;
            while (currentPrincipal != null)
            {
                NHibernateUtil.Initialize(currentPrincipal.Role);
                currentPrincipal = currentPrincipal.Role;
            }
        }
    }
}