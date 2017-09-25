using CHC.Consent.Common.Identity;

namespace CHC.Consent.NHibernate.Identity
{
    /// <summary>
    /// Represents a stored <see cref="SimpleIdentity"/>
    /// </summary>
    public class PersistedSimpleIdentity : PersistedIdentity
    {
        public virtual string Value { get; set; }

        public override bool HasSameValueAs(Common.Identity.Identity newIdentity)
        {
            return newIdentity is SimpleIdentity simpleIdentity && simpleIdentity.Value == Value;
        }
    }
}