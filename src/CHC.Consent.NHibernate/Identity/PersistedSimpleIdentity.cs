using CHC.Consent.Common.Identity;

namespace CHC.Consent.NHibernate.Identity
{
    /// <summary>
    /// Represents a stored <see cref="SimpleIdentity"/>
    /// </summary>
    public class PersistedSimpleIdentity : PersistedIdentity, ISimpleIdentity
    {
        public virtual string Value { get; set; }

        public override bool HasSameValueAs(Common.Identity.IIdentity newIdentity)
        {
            return newIdentity is ISimpleIdentity simpleIdentity && simpleIdentity.Value == Value;
        }
    }
}