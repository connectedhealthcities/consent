using System;
using CHC.Consent.Common.Consent.Evidences;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;

namespace CHC.Consent.Common.Consent
{
    public class Evidence : IIdentifier<EvidenceDefinition>, IEquatable<Evidence>
    {
        public EvidenceDefinition Definition { get; set; }
        public IIdentifierValue Value { get; set; }

        /// <inheritdoc />
        public Evidence(EvidenceDefinition definition, IIdentifierValue value)
        {
            Definition = definition;
            Value = value;
        }

        /// <inheritdoc />
        public Evidence()
        {
        }

        /// <inheritdoc />
        public bool Equals(Evidence other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Definition, other.Definition) && Equals(Value, other.Value);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Evidence) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Definition != null ? Definition.GetHashCode() : 0) * 397) ^ (Value != null ? Value.GetHashCode() : 0);
            }
        }

        public static bool operator ==(Evidence left, Evidence right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Evidence left, Evidence right)
        {
            return !Equals(left, right);
        }
    }
}