using System;
using System.Linq.Expressions;
using CHC.Consent.Identity.Core;
using CHC.Consent.NHibernate.Identity;

namespace CHC.Consent.Identity.SimpleIdentity
{
    /// <summary>
    /// Represents a stored <see cref="SimpleIdentity"/>
    /// </summary>
    public class PersistedSimpleIdentity : PersistedIdentity, ISimpleIdentity, IPersistedSimpleIdentitySource
    {
        public virtual string Value { get; set; }

        public virtual PersistedIdentity CreatePersistedIdentity()
        {
            return this;
        }

        public override bool HasSameValueAs(IIdentity newIdentity)
        {
            return newIdentity is ISimpleIdentity simpleIdentity && simpleIdentity.Value == Value;
        }
    }
}