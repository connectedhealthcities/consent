using System;
using System.Linq.Expressions;
using CHC.Consent.Identity.Core;

namespace CHC.Consent.NHibernate.Identity
{
    public static class IdentityKindHelperProviderExtensions
    {
        public static Identity CreatePersistedIdentity(
            this IIdentityKindHelperProvider @this, IIdentity identity) =>
            @this.GetHelperFor(identity).CreatePersistedIdentity(identity);

        public static Expression<Func<Identity, bool>> CreateQuery(
            this IIdentityKindHelperProvider @this, IIdentity match) =>
            @this.GetHelperFor(match).CreateMatchQuery(match);
    }
}