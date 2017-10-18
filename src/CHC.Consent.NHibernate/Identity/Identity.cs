using System;
using CHC.Consent.Identity.Core;

namespace CHC.Consent.NHibernate.Identity
{
    public abstract class Identity : Entity, IIdentity 
    {
        public virtual Guid IdentityKindId { get; set; }

        public virtual Person Person { get; set; }
        public abstract bool HasSameValueAs(IIdentity newIdentity);
    }
}