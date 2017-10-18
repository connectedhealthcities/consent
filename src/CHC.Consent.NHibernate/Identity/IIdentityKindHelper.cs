using System;
using System.Linq.Expressions;
using CHC.Consent.Identity.Core;

namespace CHC.Consent.NHibernate.Identity
{
    public interface IIdentityKindHelper
    {
        Expression<Func<Identity, bool>> CreateMatchQuery(IIdentity match);
        Identity CreatePersistedIdentity(IIdentity identity);
    }
}