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
        private readonly Func<ISession> sessionAccessor;
        private readonly IIdentityKindHelperProvider identityKindHelperProvider;
        private readonly IdentityQueryGenerator queryGenerator;

        public NHibernateIdentityStore(Func<ISession> sessionAccessor, IIdentityKindHelperProvider identityKindHelperProvider)
        {
            this.sessionAccessor = sessionAccessor;
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

        public PersistedPerson CreatePerson(IEnumerable<PersistedIdentity> identites)
        {
            var persistedPerson = new PersistedPerson(identites);
            sessionAccessor().Save(persistedPerson);
            return persistedPerson;
        }

        IPerson IIdentityStore.CreatePerson(IEnumerable<IIdentity> identities)
        {
            return CreatePerson(CreatePersistedIdentities(identities));
        }

        private IEnumerable<PersistedIdentity> CreatePersistedIdentities(IEnumerable<IIdentity> identities)
        {
            return identities.Select(identityKindHelperProvider.CreatePersistedIdentity);
        }

        private PersistedPerson Search(Expression<Func<PersistedIdentity, bool>> matchExpression)
        {
            return Search(sessionAccessor(), matchExpression);    
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