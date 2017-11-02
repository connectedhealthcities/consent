using System.Collections;
using System.Collections.Generic;
using CHC.Consent.Security;

namespace CHC.Consent.NHibernate.Security
{
    public class AccessControlList : Entity, IAccessControlList, IEnumerable<AccessControlEntry>
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
                Add(entry.Principal, entry.Permission);
            }
        }

        public virtual void Add(SecurityPrincipal principal, Permisson permission)
        {
            Entries.Add(
                new AccessControlEntry
                {
                    AccessControlList = this,
                    Principal = principal,
                    Permission = permission
                });
        }

        /// <inheritdoc />
        public virtual IEnumerator<AccessControlEntry> GetEnumerator()
        {
            return Entries.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}