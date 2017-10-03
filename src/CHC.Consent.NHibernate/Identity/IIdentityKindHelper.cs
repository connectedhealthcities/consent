using System;
using System.Linq.Expressions;
using CHC.Consent.Identity.Core;

namespace CHC.Consent.NHibernate.Identity
{
    public interface IIdentityKindHelper
    {
        Expression<Func<PersistedIdentity, bool>> CreateMatchQuery(IIdentity match);
        PersistedIdentity CreatePersistedIdentity(IIdentity identity);
    }
}