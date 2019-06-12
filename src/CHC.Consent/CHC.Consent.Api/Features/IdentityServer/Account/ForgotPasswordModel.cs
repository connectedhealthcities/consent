using System.ComponentModel.DataAnnotations;

namespace CHC.Consent.Api.Features.IdentityServer
{
    public class ForgotPasswordModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}