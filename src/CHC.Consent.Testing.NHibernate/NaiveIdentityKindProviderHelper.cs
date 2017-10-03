using System;
using CHC.Consent.Identity.Core;
using CHC.Consent.Identity.SimpleIdentity;
using CHC.Consent.NHibernate.Identity;

namespace CHC.Consent.Testing.NHibernate
{
    public class NaiveIdentityKindProviderHelper : IIdentityKindHelperProvider
    {
        /// <inheritdoc />
        public IIdentityKindHelper GetHelperFor(IIdentity identity)
        {
            if (identity is ISimpleIdentity simple)
            {
                return new SimpleIdentityKindProvider();
            }
            throw new NotImplementedException();
        }
    }
}