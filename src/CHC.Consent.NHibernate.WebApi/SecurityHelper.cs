using System;
using System.Linq;
using CHC.Consent.NHibernate.Security;
using CHC.Consent.WebApi.Abstractions;
using NHibernate;
using NHibernate.Linq;

namespace CHC.Consent.NHibernate.WebApi
{
    public class SecurityHelper
    {
        private readonly Func<ISession> getSession;

        public SecurityHelper(IUserAccessor userAccessor, Func<ISession> getSession)
        {
            UserAccessor = userAccessor;
            this.getSession = getSession;
        }

        public IUserAccessor UserAccessor { get; }

        public IQueryable<T> Readable<T>(Func<ISession, IQueryable<T>> getSecurables) where T:INHibernateSecurable
        {
            return Filter(getSecurables, Permisson.Read, Permisson.Write);
        }

        private IQueryable<T> Filter<T>(Func<ISession, IQueryable<T>> getSecurables, params string[] requiredPermissions) where T : INHibernateSecurable
        {
            var session = getSession();
            var allPrincials = AllPrincials((SecurityPrincipal) UserAccessor.GetUser());
            var acls = session.Query<AccessControlList>();
            var securables = getSecurables(session);

            return
                from securable in securables
                from acl in acls
                from ace in acl.Entries
                where requiredPermissions.Contains(ace.Permission.Name) &&
                      allPrincials.Contains(ace.Principal.Id)
                where acl.Id == securable.Acl.Id
                select securable;
        }

        private static Guid[] AllPrincials(SecurityPrincipal user)
        {
            return user.Membership().Select(_ => _.Id).ToArray();
        }

        public bool CanWriteTo<T>(T securable) where T:Entity, INHibernateSecurable
        {
            return Filter(_ => _.Query<T>(), Permisson.Write).Any(_ => _.Id == securable.Id);
        }
    }
}