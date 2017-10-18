using System;
using CHC.Consent.Identity.Core;
using CHC.Consent.NHibernate.Identity;

namespace CHC.Consent.Identity.SimpleIdentity
{
    public class SimpleIdentitySpecification : ISimpleIdentity, IPersistedSimpleIdentitySource
    {
        public SimpleIdentitySpecification(Guid identityKindId, string value)
        {
            Value = value;
            IdentityKindId = identityKindId;
        }

        public Guid IdentityKindId { get;  }
        public string Value { get;  }

        /// <inheritdoc />
        public NHibernate.Identity.Identity CreatePersistedIdentity()
        {
            return new SimpleIdentity {Value = Value, IdentityKindId = IdentityKindId};
        }
    }
}