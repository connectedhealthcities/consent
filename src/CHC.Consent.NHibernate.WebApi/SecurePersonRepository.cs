using System;
using System.Collections.Generic;
using System.Linq;
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


            var allPrincials = user.Membership().Select(_ => _.Id).ToArray();
            var people = session.Query<Person>();
            var acls = session.Query<AccessControlList>();

            var requiredPermissions = new[] {"read", "write"};

            return
                from person in people
                from acl in acls
                from ace in acl.Entries
                where requiredPermissions.Contains(ace.Permission.Name) &&
                      allPrincials.Contains(ace.Principal.Id)
                where acl.Id == person.Acl.Id
                select person;
            
        }
        
    }
}