using System.Linq;
using CHC.Consent.Identity.Core;
using CHC.Consent.NHibernate.Identity;
using CHC.Consent.WebApi.Abstractions;
using NHibernate.Linq;

namespace CHC.Consent.NHibernate.WebApi
{
    public class PersonRepository : IPersonRepository
    {
        private readonly SecurityHelper security;

        /// <inheritdoc />
        public PersonRepository(
            SecurityHelper security)
        {
            this.security = security;
        }

        /// <inheritdoc />
        public IQueryable<IPerson> GetPeople()
        {
            return security.Readable(_ => _.Query<Person>());
        }
    }
}