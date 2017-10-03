using CHC.Consent.Identity.Core;

namespace CHC.Consent.Identity.SimpleIdentity
{
    public interface ISimpleIdentity : IIdentity
    {
        string Value { get; }
    }
}