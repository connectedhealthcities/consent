using CHC.Consent.Identity.Core;

namespace CHC.Consent.Identity.SimpleIdentity
{
    /// <summary>
    /// Represents a stored <see cref="CHC.Consent.Identity.SimpleIdentity"/>
    /// </summary>
    public class SimpleIdentity : NHibernate.Identity.Identity, ISimpleIdentity, IPersistedSimpleIdentitySource
    {
        public virtual string Value { get; set; }

        public virtual NHibernate.Identity.Identity CreatePersistedIdentity()
        {
            return this;
        }

        public override bool HasSameValueAs(IIdentity newIdentity)
        {
            return newIdentity is ISimpleIdentity simpleIdentity && simpleIdentity.Value == Value;
        }
    }
}