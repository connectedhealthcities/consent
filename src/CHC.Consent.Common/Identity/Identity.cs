namespace CHC.Consent.Common.Identity
{
    public abstract class Identity
    {
        public virtual long Id { get; set; }
        public virtual IdentityKind IdentityKind { get; set; }
    }
}