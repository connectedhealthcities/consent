using CHC.Consent.Identity.Core;
using CHC.Consent.NHibernate.Identity;

namespace CHC.Consent.Identity.SimpleIdentity
{
    public interface IPersistedSimpleIdentitySource
    {
        /// <inheritdoc />
        NHibernate.Identity.Identity CreatePersistedIdentity();
    }
}