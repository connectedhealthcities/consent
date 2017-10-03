using System;
using System.Linq.Expressions;
using CHC.Consent.Identity.Core;

namespace CHC.Consent.NHibernate.Identity
{
    public static class IdentityKindHelperProviderExtensions
    {
        public static PersistedIdentity CreatePersistedIdentity(
            this IIdentityKindHelperProvider @this, IIdentity identity) =>
            @this.GetHelperFor(identity).CreatePersistedIdentity(identity);

        public static Expression<Func<PersistedIdentity, bool>> CreateQuery(
            this IIdentityKindHelperProvider @this, IIdentity match) =>
            @this.GetHelperFor(match).CreateMatchQuery(match);
    }
}