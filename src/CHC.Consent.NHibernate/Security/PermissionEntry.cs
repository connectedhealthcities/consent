using System;
using CHC.Consent.Security;

namespace CHC.Consent.NHibernate.Security
{
    public class PermissionEntry : Entity, IPermissionEntry
    {
        /// <inheritdoc />
        public virtual SecurityPrincipal Principal { get; set; }

        /// <inheritdoc />
        ISecurityPrincipal IPermissionEntry.Principal => Principal;


        /// <inheritdoc />
        public virtual Permisson Permisson { get; set; }

        /// <inheritdoc />
        IPermisson IPermissionEntry.Permisson => Permisson;

        /// <inheritdoc />
        public virtual Securable Securable { get; set; }

        /// <inheritdoc />
        ISecurable IPermissionEntry.Securable => this.Securable;
    }
}