using CHC.Consent.Core;

namespace CHC.Consent.WebApi.Abstractions.Consent
{
    public interface ISystemStore
    {
        ISystem GetSystem();
    }
}