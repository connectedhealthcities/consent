using System;
using System.Collections.Generic;
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
    using static PredicateExtensions.PredicateExtensions;

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
            var session = sessionAccessor();
            
            return session.Query<Person>()
                .Where(HasReadOrWriteAccess(user));
        }

        private Expression<Func<Person, bool>> HasReadOrWriteAccess(SecurityPrincipal principal)
            => AclGrantsReadAcccessTo<Person>(principal);
        

        private static Expression<Func<T, bool>> AclGrantsReadAcccessTo<T>(SecurityPrincipal principal)
            where T : INHibernateSecurable
        {
            var allPrincials = principal.Membership().Select(_ => _.Id).ToArray();
            return x => x.Acl.Permissions.Any(
                e => allPrincials.Contains(e.Principal.Id) && (e.Permisson.Name == "read" || e.Permisson.Name == "write"));
        }
    }
}