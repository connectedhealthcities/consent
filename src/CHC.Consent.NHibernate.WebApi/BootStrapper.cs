using System;
using System.Linq;
using CHC.Consent.NHibernate.Security;
using CHC.Consent.WebApi.Abstractions;
using CHC.Consent.WebApi.Abstractions.Bootstrap;
using NHibernate;
using NHibernate.Linq;

namespace CHC.Consent.NHibernate.WebApi
{
    using System = Consent.System;
    public class BootStrapper : IBootstrapper
    {
        public Func<ISession> GetSession { get; }
        public IUserAccessor UserAccessor { get; }

        /// <inheritdoc />
        public BootStrapper(Func<ISession> getSession, IUserAccessor userAccessor)
        {
            GetSession = getSession;
            UserAccessor = userAccessor;
        }

        /// <inheritdoc />
        public void Bootstrap()
        {
            var system = GetSession().Query<System>().SingleOrDefault();
            if (system != null)
            {
                throw new AlreadyBootstrappedException();
            }
            var readPermission =  GetOrCreatePermission(Permisson.Read);
            var writePermission =  GetOrCreatePermission(Permisson.Write);

            var user = new User {Logins = {CurrentLogin()}};
            var sysAdminRole = new Role {Description = "Provides access to entire system", Name = "System Administrator"};
            user.Role = sysAdminRole;

            GetSession().Save(sysAdminRole);
            GetSession().Save(user);

            system = new System {Acl = {{sysAdminRole, readPermission}, {sysAdminRole, writePermission}}};
            
            GetSession().Save(system);

            GetSession().Flush();
        }

        private JwtLogin CurrentLogin()
        {
            var (issuer, subject) = UserAccessor.GetJwtCredentials();
            return new JwtLogin(issuer, subject);
        }

        private Permisson GetOrCreatePermission(string name)
        {
            var permission = GetSession().Query<Permisson>().SingleOrDefault(_ => _.Name == name);
            if (permission == null)
            {
                GetSession().Save(permission = new Permisson {Name = name});
            }
            return permission;
        }
    }

    public class AlreadyBootstrappedException : Exception
    {
    }
}