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

        /// <summary>
        /// Gets all Principals, including this one, to which this Principal belongs
        /// </summary>
        /// <remarks>Assumes either the session is still active or (better still) the role hierachy has already been loaded</remarks>
        public virtual IEnumerable<SecurityPrincipal> Membership()
        {
            var principal = this;
            while (principal != null)
            {
                yield return principal;
                principal = principal.Role;
            }
        }
    }
    
    
}