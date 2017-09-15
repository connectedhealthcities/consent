namespace CHC.Consent.Common.Identity
{
    public class IdentityKind
    {
        public IdentityKindFormat Format { get; set; }
        public bool Key { get; set; }
        public string Id { get; set; }
    }

    /// <summary>
    /// Identifies the format of Identities
    /// </summary>
    public enum IdentityKindFormat
    {
        /// <summary>
        /// Indicates that the Identity is a simple/single value - used for dates, numbers, etc
        /// </summary>
        Simple = 1,
        /// <summary>
        /// Indicates that the Indentity is a composite identity - used for addresses, and other multi-value identities
        /// </summary>
        Composite = 2
    }
}