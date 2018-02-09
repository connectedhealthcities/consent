using System;
using System.Linq.Expressions;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.IdentifierTypes;

namespace CHC.Consent.Common
{
    /// <remarks>It's easier to inherit from <see cref="IdentifierType{TValue}"/></remarks>
    public abstract class IdentifierType
    {
        public string ExternalId { get; }

        public bool CanHaveMultipleValues { get; }

        public IdentifierValueType ValueType { get; }

        /// <inheritdoc />
        public IdentifierType(string externalId, bool canHaveMultipleValues, IdentifierValueType valueType)
        {
            ExternalId = externalId;
            CanHaveMultipleValues = canHaveMultipleValues;
            ValueType = valueType;
        }

        public virtual Identifier Parse(string representation)
        {
            return new Identifier(this, ValueType, ValueType.Parse(representation));
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

        public static IdentifierType NhsNumber { get; } = new NhsNumber();
        public static IdentifierType BradfordHospitalNumber { get; } = new BradfordHospitalNumber();
        public abstract Expression<Func<Person, bool>> GetMatchExpression(IdentifierValue value);
        public abstract void Update(Person person, IdentifierValue value);
    }

    public abstract class IdentifierType<TValue> : IdentifierType where TValue : IdentifierValue
    {
        /// <inheritdoc />
        protected IdentifierType(string externalId, bool canHaveMultipleValues, IdentifierValueType valueType) 
            : base(externalId, canHaveMultipleValues, valueType)
        {
        }

        /// <inheritdoc />
        public override Expression<Func<Person, bool>> GetMatchExpression(IdentifierValue value)
        {
            return GetMatchExpression(ConvertToCorrectType(value));
        }

        private static TValue ConvertToCorrectType(IdentifierValue value)
        {
            switch (value)
            {
                case TValue typedValue:
                    return typedValue;
                case null:
                    throw new ArgumentNullException(nameof(value), $"Cannot convert null to {typeof(TValue)}");
                default:
                    throw new InvalidOperationException(
                        $"Expecting a value of type {typeof(TValue)} but got {value?.GetType()}");
            }
        }

        protected abstract Expression<Func<Person, bool>> GetMatchExpression(TValue value);

        /// <inheritdoc />
        public override void Update(Person person, IdentifierValue value)
        {
            Update(person, ConvertToCorrectType(value));
        }

        protected abstract void Update(Person person, TValue value);
    }
}