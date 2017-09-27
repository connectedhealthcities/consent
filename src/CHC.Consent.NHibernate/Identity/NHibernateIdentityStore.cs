using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using System.Xml.Serialization;
using CHC.Consent.Common.Core;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Import.Match;
using CHC.Consent.Common.Utils;
using CHC.Consent.Identity.Core;
using NHibernate;
using NHibernate.Linq;
using PredicateExtensions;

namespace CHC.Consent.NHibernate.Identity
{
    public class NHibernateIdentityStore : IIdentityStore
    {
        private readonly ISessionFactory sessionFactory;

        public NHibernateIdentityStore(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
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
                    var persistedPerson = new PersistedPerson(identities);

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
            var matched = s.Query<PersistedIdentity>()
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

        private static Expression<Func<PersistedIdentity, bool>> CreateMatchQuery(IMatch match)
        {
            
            
            Expression<Func<PersistedIdentity, bool>> matchExpression = null;
            if (match is IIdentityMatch matchByIdentity)
            {
                //TODO: error handling
                //TODO: Composite identity matching

                if (matchByIdentity.Match is ISimpleIdentity simpleIdentity)
                {
                    Expression<Func<PersistedIdentity, bool>> identityIdMatch = id => id.IdentityKindId == simpleIdentity.IdentityKindId;

                    var identityValue = simpleIdentity.Value;


                    matchExpression = identityIdMatch.And(
                        id =>
                            id is PersistedSimpleIdentity &&
                            ((PersistedSimpleIdentity) id).Value == identityValue);
                }
                else
                {
                    throw new NotImplementedException("Can only match by simple identity");
                }
            }
            else
            {
                //TODO: Match other matches - identity by Id and Logical Matches 
                throw new NotImplementedException();
            }
            return matchExpression;
            
        }
    }
}