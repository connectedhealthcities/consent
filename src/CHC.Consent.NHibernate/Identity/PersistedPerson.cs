using System;
using System.Collections.Generic;

namespace CHC.Consent.NHibernate.Identity
{
    public class PersistedPerson
    {
        public virtual Guid Id { get; set; }
        public virtual ICollection<PersistedIdentity> Identities { get; set; } = new List<PersistedIdentity>();
    }
}