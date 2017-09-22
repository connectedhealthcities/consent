using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Import.Match;
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
                Expression<Func<PersistedIdentity, bool>> matchExpression = null;
                if (match.GetType() == typeof(IdentityKindId))
                {
                    //TODO: error handling
                    //TODO: Composite identity matching
                    var externalId = ((IdentityKindId) match).Id;

                    Expression<Func<PersistedIdentity, bool>> identityIdMatch = id => id.IdentityKind.ExternalId == externalId;
                    
                    var identityValue = ((SimpleIdentity)identities.First(_ => _.IdentityKind.ExternalId == externalId)).Value;

                    
                    
                    matchExpression = identityIdMatch.And(id => 
                        id is PersistedSimpleIdentity && 
                        ((PersistedSimpleIdentity) id).Value == identityValue); 

                    
                }
                else
                {
                    //TODO: other matches
                    throw new NotImplementedException();
                }
                
                var identitySets = sessionFactory.AsTransaction(
                    s =>
                    {
                        var matched = s.Query<PersistedIdentity>()
                            .Where(matchExpression)
                            .Select(_ => _.Person);
                            
                            
                        matched.FetchMany(_ => _.Identities).ThenFetch(i => i.IdentityKind).ToFuture();
                        
                        return matched.Select(_ => _.Identities).ToFuture().ToArray();
                    });
                    
                //TODO: Handle more than one person?
                if(identitySets.Length > 1) throw new NotImplementedException();

                //TODO: handle Composite identies mapping
                return identitySets[0].Select(_ => new SimpleIdentity{IdentityKind = _.IdentityKind, Id = _.Id, Value = ((PersistedSimpleIdentity)_).Value });
            }
            //TODO: Watch happens if no one is found
            throw new NotImplementedException();
        }

        public void UpsertIdentity(IReadOnlyCollection<Match> match, IEnumerable<Common.Identity.Identity> allIdentities)
        {
            throw new System.NotImplementedException();
        }
    }
}