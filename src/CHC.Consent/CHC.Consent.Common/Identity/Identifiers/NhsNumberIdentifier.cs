using System;
using System.Linq.Expressions;

namespace CHC.Consent.Common.Identity.Identifiers
{
    [Identifier(TypeName)]
    public class NhsNumberIdentifier : IIdentifier, ISingleValueIdentifier<string>
    {
        public const string TypeName = "uk.nhs.nhs-number";

        private readonly SingleValueIdentifierHelper<string> helper
            = new SingleValueIdentifierHelper<string>(_ => _.NhsNumber);

        /// <inheritdoc />
        public NhsNumberIdentifier(string value = null) 
        {
            Value = value;
        }

        public string Value { get; }


        /// <inheritdoc />
        public Expression<Func<Person, bool>> GetMatchExpression()
            => helper.GetMatchExpression(Value);

        /// <inheritdoc />
        public void Update(Person person) => helper.Update(person, Value);


        protected bool Equals(NhsNumberIdentifier other)
        {
            return string.Equals(Value, other.Value);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NhsNumberIdentifier) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }
    }
}