using System;
using System.Globalization;
using Microsoft.AspNetCore.Identity;

namespace CHC.Consent.EFCore.Security
{
    public class ConsentUser : IdentityUser<long>
    {
        public UserSecurityPrincipal Principal { get; protected set; } = new UserSecurityPrincipal();
        
        public DateTime? Deleted { get; set; }

        public string SubjectId => Id.ToString(CultureInfo.InvariantCulture);
    }
}