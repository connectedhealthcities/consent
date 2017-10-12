using System;
using System.Linq;
using System.Linq.Expressions;
using CHC.Consent.Identity.Core;
using CHC.Consent.NHibernate.Identity;
using CHC.Consent.NHibernate.Security;
using CHC.Consent.WebApi.Abstractions;
using NHibernate;
using NHibernate.Linq;
using PredicateExtensions;

namespace CHC.Consent.NHibernate.WebApi
{
    public class SecurePersonRepository : IPersonRepository
    {
        public IUserAccessor UserAccessor { get; }
        private readonly INHibernatePersonRespository inner;
        private readonly Func<ISession> getSession;

        public ISession Session => getSession();

        /// <inheritdoc />
        public SecurePersonRepository(
            INHibernatePersonRespository inner,
            IUserAccessor userAccessor,
            Func<ISession> getSession)
        {
            UserAccessor = userAccessor;
            this.inner = inner;
            this.getSession = getSession;
        }

        /// <inheritdoc />
        public IQueryable<IPerson> GetPeople()
        {
            var user = (User)UserAccessor.GetUser();
            return inner.GetPeople().Where(HasExplicitAccess(user).Or(HasAccessViaStudy(user)));
        }

        private Expression<Func<PersistedPerson, bool>> HasAccessViaStudy(SecurityPrincipal principal)
        {
            var accessibleStudies =
                Session.Query<SecurableStudy>()
                    .Where(GivesAccessTo<SecurableStudy>(principal));
            
            return person => accessibleStudies
                .Any(s => person.SubjectIdentifiers.Any(si => si.StudyId == s.Study.Id));
        }

        private Expression<Func<PersistedPerson, bool>> HasExplicitAccess(SecurityPrincipal principal)
        {
            var accessiblePeople = Session.Query<SecurablePerson>().Where(GivesAccessTo<SecurablePerson>(principal));

            return person => accessiblePeople.Any(s => s.Person == person);
        }

        private Expression<Func<T, bool>> GivesAccessTo<T>(SecurityPrincipal principal)
            where T : Securable =>
            x => x.Entries.Any(
                e => e.Principal.Id == principal.Id &&
                     (e.Permisson.Name == "read" || e.Permisson.Name == "write"));
    }
}