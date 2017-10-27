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

        public AccessControlList(INHibernateSecurable owner, AccessControlList source) : this(owner)
        {
            CopyFrom(source);
        }

        public virtual ICollection<AccessControlEntry> Entries { get; protected set; } = new List<AccessControlEntry>();
        /// <inheritdoc />
        IEnumerable<IAccessControlEntry> IAccessControlList.Permissions => Entries;

        public virtual INHibernateSecurable Owner { get; set; }

        public virtual void CopyFrom(AccessControlList source)
        {
            foreach (var entry in source.Entries)
            {
                Entries.Add(
                    new AccessControlEntry
                    {
                        AccessControlList = this,
                        Principal = entry.Principal,
                        Permission = entry.Permission
                    });
            }
        }
    }
}