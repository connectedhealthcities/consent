using System.Collections.Generic;
using CHC.Consent.Security;

namespace CHC.Consent.NHibernate.Security
{
    public class SecurityPrincipal : Entity, ISecurityPrincipal
    {
        public virtual Role Role { get; set; }

        /// <inheritdoc />
        IRole ISecurityPrincipal.Role => Role;

        public virtual ICollection<PermissionEntry> PermissionEntries { get; protected set; } =
            new List<PermissionEntry>();

        /// <inheritdoc />
        IEnumerable<IPermissionEntry> ISecurityPrincipal.PermissionEntries => PermissionEntries;
    }
    
    
}