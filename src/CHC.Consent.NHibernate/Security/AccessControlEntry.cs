using System;
using CHC.Consent.Security;

namespace CHC.Consent.NHibernate.Security
{
    public class AccessControlEntry : Entity, IAccessControlEntry
    {
        /// <inheritdoc />
        public virtual SecurityPrincipal Principal { get; set; }

        /// <inheritdoc />
        ISecurityPrincipal IAccessControlEntry.Principal => Principal;


        /// <inheritdoc />
        public virtual Permisson Permission { get; set; }

        /// <inheritdoc />
        IPermisson IAccessControlEntry.Permission => Permission;

        /// <inheritdoc />
        public virtual AccessControlList AccessControlList { get; set; }

        /// <inheritdoc />
        IAccessControlList IAccessControlEntry.AccessControlList => AccessControlList;
    }
}