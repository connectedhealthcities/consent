namespace CHC.Consent.Identity.Core
{
    public interface IIdentityMatch : IMatch
    {
        IIdentity Match { get; }
    }
}