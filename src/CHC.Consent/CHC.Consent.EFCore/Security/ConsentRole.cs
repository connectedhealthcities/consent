using Microsoft.AspNetCore.Identity;

namespace CHC.Consent.EFCore.Security
{
    public class ConsentRole : IdentityRole<long>
    {
        public ConsentRole ParentRole { get; set; }
        public RoleSecurityPrincipal Principal { get; private set; } = new RoleSecurityPrincipal();
    }
}