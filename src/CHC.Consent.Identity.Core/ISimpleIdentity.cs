namespace CHC.Consent.Identity.Core
{
    public interface ISimpleIdentity : IIdentity
    {
        string Value { get; }
    }
}