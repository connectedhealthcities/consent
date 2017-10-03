using CHC.Consent.Identity.Core;

namespace CHC.Consent.NHibernate.Identity
{
    public interface IIdentityKindHelperProvider
    {
        IIdentityKindHelper GetHelperFor(IIdentity identity);
    }
}