using Microsoft.AspNetCore.Identity;

namespace CHC.Consent.EFCore.Security
{
    public class ConsentUser : IdentityUser
    {
        public UserSecurityPrincipal Principal { get; protected set; } = new UserSecurityPrincipal();
    }
}