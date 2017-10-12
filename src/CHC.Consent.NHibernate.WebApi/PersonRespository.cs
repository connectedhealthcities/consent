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
        private readonly Func<ISession> db;

        /// <inheritdoc />
        public PersonRespository(Func<ISession> db)
        {
            this.db = db;
        }

        /// <inheritdoc />
        public virtual IQueryable<PersistedPerson> GetPeople()
        {
            return db().Query<PersistedPerson>();
        }
    }
}