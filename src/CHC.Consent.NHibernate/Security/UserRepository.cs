using System.Linq;
using CHC.Consent.Security;
using NHibernate.Linq;

namespace CHC.Consent.NHibernate.Security
{
    public class UserRepository : IJwtIdentifiedUserRepostiory
    {
        private ISessionFactory SessionFactory { get; }

        /// <inheritdoc />
        public UserRepository(ISessionFactory sessionFactory)
        {
            SessionFactory = sessionFactory;
        }

        /// <inheritdoc />
        public IUser FindUserBy(string issuer, string subject)
        {
            return SessionFactory.AsTransaction(
                s => s.Query<User>().SingleOrDefault(
                    user =>
                        user.Logins
                            .OfType<JwtLogin>()
                            .Any(id => id.Issuer == issuer && id.Subject == subject)));
        }
    }
}