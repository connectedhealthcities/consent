using System;
using CHC.Consent.Identity.Core;

namespace CHC.Consent.Common.Identity
{
    public class IdentityKind : IIdentityKind
    {
        public virtual IdentityKindFormat Format { get; set; }
        public virtual string ExternalId { get; set; }
        
        public virtual Guid Id { get; protected set; }
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