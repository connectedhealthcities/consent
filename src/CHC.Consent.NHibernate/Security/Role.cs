using System;
using CHC.Consent.Security;

namespace CHC.Consent.NHibernate.Security
{
    public class Role : SecurityPrincipal, IRole
    {
        /// <inheritdoc />
        public virtual string Description { get; set; }

        /// <inheritdoc />
        public virtual string Name { get; set; }

        /// <inheritdoc />
        protected override bool HasSameBusinessValueAs(Entity compareTo)
        {
            return compareTo is Role role && string.CompareOrdinal(Name, role.Name) == 0;
        }
    }
}