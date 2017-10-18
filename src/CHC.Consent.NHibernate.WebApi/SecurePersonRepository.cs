using System;
using System.Linq;
using System.Linq.Expressions;
using CHC.Consent.Identity.Core;
using CHC.Consent.NHibernate.Identity;
using CHC.Consent.NHibernate.Security;
using CHC.Consent.WebApi.Abstractions;
using NHibernate;
using NHibernate.Linq;

namespace CHC.Consent.NHibernate.WebApi
{
    public class SecurePersonRepository : IPersonRepository
    {
        public IUserAccessor UserAccessor { get; }
        private readonly Func<ISession> sessionAccessor;

        public ISession Session => sessionAccessor();

        /// <inheritdoc />
        public SecurePersonRepository(
            IUserAccessor userAccessor,
            Func<ISession> sessionAccessor)
        {
            UserAccessor = userAccessor;
            this.sessionAccessor = sessionAccessor;
        }

        /// <inheritdoc />
        public IQueryable<IPerson> GetPeople()
        {
            var user = (User)UserAccessor.GetUser();
            return sessionAccessor().Query<Person>()
                .Where(HasExplicitAccess(user));
        }

        private Expression<Func<Person, bool>> HasExplicitAccess(SecurityPrincipal principal)
        {
            return AclGrantsReadAcccessTo<Person>(principal);
        }

        private Expression<Func<T, bool>> AclGrantsReadAcccessTo<T>(SecurityPrincipal principal)
            where T : INHibernateSecurable =>
            x => x.Acl.Permissions.Any(e => e.Principal.Id == principal.Id && (e.Permisson.Name == "read" || e.Permisson.Name == "write"));
    }
}