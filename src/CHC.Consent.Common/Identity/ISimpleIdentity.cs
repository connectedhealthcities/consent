namespace CHC.Consent.Common.Identity
{
    public interface ISimpleIdentity : IIdentity
    {
        string Value { get; }
    }
}