using CHC.Consent.Identity.Core;

namespace CHC.Consent.NHibernate.Identity
{
    /// <summary>
    /// Represents a stored <see cref="SimpleIdentity"/>
    /// </summary>
    public class PersistedSimpleIdentity : PersistedIdentity, ISimpleIdentity
    {
        public virtual string Value { get; set; }

        public override bool HasSameValueAs(IIdentity newIdentity)
        {
            return newIdentity is ISimpleIdentity simpleIdentity && simpleIdentity.Value == Value;
        }
    }
}