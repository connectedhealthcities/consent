using System;
using CHC.Consent.Common.Identity;

namespace CHC.Consent.NHibernate.Identity
{
    public abstract class PersistedIdentity
    {
        public virtual long Id { get; set; }
        public virtual IdentityKind IdentityKind { get; set; }
        public virtual PersistedPerson Person { get; set; }
    }
}