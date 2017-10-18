using System.Collections.Generic;
using CHC.Consent.Security;

namespace CHC.Consent.NHibernate.Security
{
    public class SecurityPrincipal : Entity, ISecurityPrincipal
    {
        public virtual Role Role { get; set; }

        /// <inheritdoc />
        IRole ISecurityPrincipal.Role => Role;

        public virtual ICollection<AccessControlEntry> PermissionEntries { get; protected set; } =
            new List<AccessControlEntry>();

        /// <inheritdoc />
        IEnumerable<IAccessControlEntry> ISecurityPrincipal.PermissionEntries => PermissionEntries;
    }
    
    
}