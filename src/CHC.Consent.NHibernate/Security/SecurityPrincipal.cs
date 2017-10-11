using System.Collections.Generic;
using CHC.Consent.Security;

namespace CHC.Consent.NHibernate.Security
{
    public class SecurityPrincipal : Entity, ISecurityPrincipal
    {
        public virtual Role Role { get; set; }

        /// <inheritdoc />
        IRole ISecurityPrincipal.Role => Role;

        /// <inheritdoc />
        public virtual IEnumerable<IPermissionEntry> PermissionEntries { get; set; }
    }
}