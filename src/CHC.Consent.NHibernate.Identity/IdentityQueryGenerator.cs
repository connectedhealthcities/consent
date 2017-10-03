using System;
using System.Linq.Expressions;
using CHC.Consent.Identity.Core;

namespace CHC.Consent.NHibernate.Identity
{
    public class IdentityQueryGenerator
    {
        private readonly IIdentityKindHelperProvider identityKindHelperProvider;

        public IdentityQueryGenerator(IIdentityKindHelperProvider identityKindHelperProvider)
        {
            this.identityKindHelperProvider = identityKindHelperProvider;
        }
        
        public Expression<Func<PersistedIdentity, bool>> CreateMatchQuery(IMatch match)
        {
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