namespace CHC.Consent.Common.Identity
{
    public interface IIdentifierTypeRegistry
    {
        IdentifierType GetIdentifierType(string externalId);
    }
}