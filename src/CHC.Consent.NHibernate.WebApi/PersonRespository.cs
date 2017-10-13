using System;
using System.Linq;
using CHC.Consent.NHibernate.Identity;
using NHibernate;
using NHibernate.Linq;

namespace CHC.Consent.NHibernate.WebApi
{
    public interface INHibernatePersonRespository
    {
        /// <inheritdoc />
        IQueryable<PersistedPerson> GetPeople();
    }

    public class PersonRespository : INHibernatePersonRespository
    {
        private readonly Func<ISession> sessionAccessor;

        /// <inheritdoc />
        public PersonRespository(Func<ISession> sessionAccessor)
        {
            this.sessionAccessor = sessionAccessor;
        }

        /// <inheritdoc />
        public virtual IQueryable<PersistedPerson> GetPeople()
        {
            return sessionAccessor().Query<PersistedPerson>();
        }
    }
}