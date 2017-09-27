namespace CHC.Consent.Identity.Core
{
    public interface IIdentityKindStore
    {
        IIdentityKind FindIdentityKindByExternalId(string externalId);
    }
}