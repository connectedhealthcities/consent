using System.Security.Claims;

namespace CHC.Consent.Api.Features.IdentityServer
{
    public class ForgotPasswordEmailModel
    {
        public ClaimsPrincipal User { get; set; }
        public string Token { get; set; }
        public string Url { get; set; }
    }
}