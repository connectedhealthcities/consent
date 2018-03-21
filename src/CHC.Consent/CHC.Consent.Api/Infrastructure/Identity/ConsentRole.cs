using Microsoft.AspNetCore.Identity;

namespace CHC.Consent.Api.Infrastructure.Identity
{
    public class ConsentRole : IdentityRole
    {
        public ConsentRole ParentRole { get; set; }
    }
}