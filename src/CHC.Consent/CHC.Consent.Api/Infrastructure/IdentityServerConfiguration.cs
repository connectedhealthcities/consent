using JetBrains.Annotations;

namespace CHC.Consent.Api.Infrastructure
{
    [UsedImplicitly]
    public class IdentityServerConfiguration
    {
        public string Authority { get; set; }
        public bool EnableInternalServer { get; set; } = true;
    }
}