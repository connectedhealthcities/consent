using System;
using CHC.Consent.Common.Identity;

namespace CHC.Consent.Common
{
    public class IdentifierType
    {
        public int Id { get; }

        public string ExternalId { get; }

        public bool Unique { get; }

        public IdentifierValueType ValueType { get; }

        /// <inheritdoc />
        public IdentifierType(int id, string externalId, bool unique, IdentifierValueType valueType)
        {
            Id = id;
            ExternalId = externalId;
            Unique = unique;
            ValueType = valueType;
        }

        public Identifier Parse(string representation)
        {
            return ValueType.Parse(representation, this);
        }

        protected bool Equals(IdentifierType other)
        {
            return string.Equals(ExternalId, other.ExternalId, StringComparison.InvariantCulture);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IdentifierType) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (ExternalId != null ? StringComparer.InvariantCulture.GetHashCode(ExternalId) : 0);
        }

        public static readonly IdentifierType NhsNumber = new IdentifierType(
            id: 1,
            externalId: "nhs.uk/nhs-number",
            unique: true,
            valueType: new IdentifierValueStringType());

        public static IdentifierType BradfordHospitalNumber { get; } = new IdentifierType(
            id: 2,
            externalId: "bradfordhospitals.nhs.uk/hosptial-id",
            unique: false,
            valueType: new IdentifierValueStringType());
    }
}