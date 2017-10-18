using System.Collections.Generic;

namespace CHC.Consent.Security
{
    public interface ISecurityPrincipal 
    {
        IRole Role { get; }
        
        IEnumerable<IAccessControlEntry> PermissionEntries { get; }
    }
}