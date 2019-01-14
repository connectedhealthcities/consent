using Microsoft.Extensions.Configuration;

namespace CHC.Consent.Api.Infrastructure
{
    public static class IdentityServerConfigurationExtensions
    {
        public static IdentityServerConfiguration GetIdentityServer(
            this IConfiguration configuration, string sectionName = "IdentityServer")
        {
            return configuration.GetSection(sectionName).Get<IdentityServerConfiguration>();
        }
    }
}