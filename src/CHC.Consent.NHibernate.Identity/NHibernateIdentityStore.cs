using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CHC.Consent.Identity.Core;
using NHibernate;
using NHibernate.Linq;

namespace CHC.Consent.NHibernate.Identity
{
    public class NHibernateIdentityStore : IIdentityStore
    {
        private readonly ISessionFactory sessionFactory;
        private readonly IIdentityKindHelperProvider identityKindHelperProvider;
        private readonly IdentityQueryGenerator queryGenerator;

        public NHibernateIdentityStore(ISessionFactory sessionFactory, IIdentityKindHelperProvider identityKindHelperProvider)
        {
            this.sessionFactory = sessionFactory;
            this.identityKindHelperProvider = identityKindHelperProvider;
            queryGenerator = new IdentityQueryGenerator(identityKindHelperProvider);
        }

        public IPerson FindPerson(IReadOnlyCollection<IMatch> matches)
        {
            return matches
                .Select(queryGenerator.CreateMatchQuery)
                .Select(Search)
                .FirstOrDefault(matchedPerson => matchedPerson != null);
        }

        public IPerson CreatePerson(IEnumerable<IIdentity> identities)
        {
            return sessionFactory.AsTransaction(
                s =>
                {
                    var persistedPerson = new PersistedPerson(CreatePersistedIdentities(identities));

                    s.Save(persistedPerson);

                    return persistedPerson;
                });
        }

        private IEnumerable<PersistedIdentity> CreatePersistedIdentities(IEnumerable<IIdentity> identities)
        {
            return identities.Select(identityKindHelperProvider.CreatePersistedIdentity);
        }

        private PersistedPerson Search(Expression<Func<PersistedIdentity, bool>> matchExpression)
        {
            return sessionFactory.AsTransaction(_ => Search(_, matchExpression));    
        }

        private PersistedPerson Search(ISession s, Expression<Func<PersistedIdentity, bool>> matchExpression)
        {
            var matched = 
                s.Query<PersistedIdentity>()
                    .Where(matchExpression)
                    .Select(_ => _.Person);

            if (matched.LongCount() > 1)
            {
                //TODO: Handle finding than one person?
                throw new NotImplementedException("We don't know how to deal with more than one match yet");
            }

            return matched.FetchMany(_ => _.Identities).ToFuture().SingleOrDefault();

            //TODO: optimise this for the case where they are more than a few matches!
        }
    }
}