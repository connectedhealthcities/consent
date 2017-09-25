using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using System.Xml.Serialization;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Import.Match;
using CHC.Consent.Common.Utils;
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

        public IEnumerable<Common.Identity.Identity> FindExisitingIdentiesFor(IReadOnlyCollection<Match> matches, IEnumerable<Common.Identity.Identity> identities)
        {
            foreach (var match in matches)
            {
                var matchExpression = CreateMatchQuery(match, identities);

                var matchedPerson = Search<ICollection<PersistedIdentity>>(matchExpression);
                    
                if(matchedPerson == null) continue;

                return matchedPerson.Identities.Select(CreateDomainIdentity);
            }
            //TODO: Watch happens if no one is found
            throw new NotImplementedException("Don't know how to handle more than one identity");
        }

        private static Common.Identity.Identity CreateDomainIdentity(PersistedIdentity identity)
        {
            //TODO: handle Composite identies mapping
            return new SimpleIdentity{IdentityKind = identity.IdentityKind, Id = identity.Id, Value = ((PersistedSimpleIdentity)identity).Value };
        }

        public void UpsertIdentity(IReadOnlyCollection<Match> matches, IEnumerable<Common.Identity.Identity> allIdentities)
        {
            foreach (var match in matches)
            {
                var query = CreateMatchQuery(match, allIdentities);
                if (sessionFactory.AsTransaction(
                    s =>
                    {
                        var matchedPerson = Search<PersistedPerson>(s, query);

                        if (matchedPerson == null) return false;

                        var person = matchedPerson;
                        UpdatePersonIdentities(person, allIdentities, s);

                        return true;
                    }))
                {
                    break;
                }
            }
        }

        private void UpdatePersonIdentities(PersistedPerson person, IEnumerable<Common.Identity.Identity> allIdentities, ISession session)
        {
            foreach (var newIdentity in allIdentities)
            {
                //TODO: updating identites - are some immutable? What about composites?
                if (person.Identities.Any(i => i.HasSameValueAs(newIdentity))) continue;
                var persistedIdentity = CreatePersistedIdentityFor(newIdentity, session);
                person.Identities.Add(persistedIdentity);
                persistedIdentity.Person = person;
            }
        }

        private PersistedIdentity CreatePersistedIdentityFor(Common.Identity.Identity identity, ISession session)
        {
            if(identity.GetType() == typeof(SimpleIdentity))
            {
                //TODO: Can we dynamically create identity kinds? Or is this an error condition...
                var identityKind = session.Query<IdentityKind>()
                    .Single(_ => _.ExternalId == identity.IdentityKind.ExternalId);

                return new PersistedSimpleIdentity
                {
                    IdentityKind = identityKind,
                    Id = identity.Id,
                    Value = ((SimpleIdentity) identity).Value
                };
            }
            
            //TODO: handle persisting non-simple identities
            throw new NotImplementedException();
        }
        

        private PersistedPerson Search<T>(Expression<Func<PersistedIdentity, bool>> matchExpression)
        {
            return sessionFactory.AsTransaction(_ => Search<T>(_, matchExpression));    
        }

        private PersistedPerson Search<T>(ISession s, Expression<Func<PersistedIdentity, bool>> matchExpression)
        {
            var matched = s.Query<PersistedIdentity>()
                .Where(matchExpression)
                .Select(_ => _.Person);

            if (matched.LongCount() > 1)
            {
                //TODO: Handle finding than one person?
                throw new NotImplementedException("We don't know how to deal with more than one match yet");
            }

            return matched.FetchMany(_ => _.Identities).ThenFetch(i => i.IdentityKind).ToFuture().SingleOrDefault();

            //TODO: optimise this for the case where they are more than a few matches!

            
        }

        private static Expression<Func<PersistedIdentity, bool>> CreateMatchQuery(Match match, IEnumerable<Common.Identity.Identity> identities)
        {
            Expression<Func<PersistedIdentity, bool>> matchExpression = null;
            if (match.GetType() == typeof(IdentityKindId))
            {
                //TODO: error handling
                //TODO: Composite identity matching
                var externalId = ((IdentityKindId) match).Id;

                Expression<Func<PersistedIdentity, bool>> identityIdMatch = id => id.IdentityKind.ExternalId == externalId;

                var identityValue = ((SimpleIdentity) identities.First(_ => _.IdentityKind.ExternalId == externalId)).Value;


                matchExpression = identityIdMatch.And(
                    id =>
                        id is PersistedSimpleIdentity &&
                        ((PersistedSimpleIdentity) id).Value == identityValue);
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