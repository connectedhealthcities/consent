using System.Collections.Generic;
using CHC.Consent.NHibernate.Consent;
using CHC.Consent.NHibernate.Identity;
using CHC.Consent.Security;

namespace CHC.Consent.NHibernate.Security
{
    public abstract class Securable : Entity, ISecurable
    {
        public virtual  IEnumerable<PermissionEntry> Entries { get; protected set; } = new List<PermissionEntry>();
    }

    public class SecurableStudy : Securable
    {
        public virtual Study Study { get; set; }
    }

    public class SecurablePerson : Securable
    {
        public virtual PersistedPerson Person { get; set; }
    }
}