using System.Collections.Generic;
using CHC.Consent.Security;

namespace CHC.Consent.NHibernate.Security
{
    public class AccessControlList : Entity, IAccessControlList
    {
        /// <remarks>persistence</remarks>
        protected AccessControlList()
        {
        }

        /// <inheritdoc />
        public AccessControlList(INHibernateSecurable owner)
        {
            Owner = owner;
        }

        public virtual ICollection<AccessControlEntry> Entries { get; protected set; } = new List<AccessControlEntry>();
        /// <inheritdoc />
        IEnumerable<IAccessControlEntry> IAccessControlList.Permissions => Entries;

        public virtual INHibernateSecurable Owner { get; set; }
    }
}