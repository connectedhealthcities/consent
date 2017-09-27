namespace CHC.Consent.Common.Identity
{
    public interface IIdentity
    {
        long Id { get; }
        IdentityKind IdentityKind { get; }
    }
}