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

        public NHibernateIdentityStore(ISessionFactory sessionFactory, IIdentityKindHelperProvider identityKindHelperProvider)
        {
            this.sessionFactory = sessionFactory;
            this.identityKindHelperProvider = identityKindHelperProvider;
        }

        public IPerson FindPerson(IReadOnlyCollection<IMatch> matches)
        {
            return matches
                .Select(CreateMatchQuery)
                .Select(Search)
                .FirstOrDefault(matchedPerson => matchedPerson != null);
        }

        public IPerson CreatePerson(IEnumerable<IIdentity> identities)
        {
            return sessionFactory.AsTransaction(
                s =>
                {
                    var persistedPerson = new PersistedPerson(identities.Select(identityKindHelperProvider.CreatePersistedIdentity));

                    s.Save(persistedPerson);

                    return persistedPerson;
                });
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

        private Expression<Func<PersistedIdentity, bool>> CreateMatchQuery(IMatch match)
        {
             Expression<Func<IIdentity, bool>> matchExpression;
            if (match is IIdentityMatch matchByIdentity)
            {
                //TODO: error handling
                //TODO: Composite identity matching

                return identityKindHelperProvider.CreateQuery(matchByIdentity.Match);
            }
            else
            {
                //TODO: Match other matches - identity by Id and Logical Matches 
                throw new NotImplementedException();
            }   
        }
    }
}