using System;
using CHC.Consent.Identity.Core;

namespace CHC.Consent.NHibernate.Identity
{
    public abstract class PersistedIdentity : IIdentity 
    {
        public virtual long Id { get; set; }
        public virtual Guid IdentityKindId { get; set; }
        public virtual PersistedPerson Person { get; set; }
        public abstract bool HasSameValueAs(IIdentity newIdentity);
    }
}