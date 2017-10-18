using System;
using CHC.Consent.Identity.Core;

namespace CHC.Consent.NHibernate.Consent
{
    public class IdentityKind : Entity, IIdentityKind
    {
        public virtual IdentityKindFormat Format { get; set; }
        public virtual string ExternalId { get; set; }
    }
}