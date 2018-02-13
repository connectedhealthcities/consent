using System;
using System.Linq.Expressions;
using CHC.Consent.Common.Identity.IdentifierValues;

namespace CHC.Consent.Common.Identity.Identifiers
{
    public class NhsNumberIdentifier : Identifier
    {
        private readonly SingleValueIdentifierTypeHelper<StringIdentifierValue, string> helper;
        /// <inheritdoc />
        public NhsNumberIdentifier(string nhsNumber = null) : base(
            IdentifierType.NhsNumber,
            new StringIdentifierValue(nhsNumber))
        {
            helper = new SingleValueIdentifierTypeHelper<StringIdentifierValue, string>(_ => _.NhsNumber);
        }

        public string Value
        {
            get => ((StringIdentifierValue) base.Value)?.Value;
            set => base.Value = new StringIdentifierValue(value);
        }

        /// <inheritdoc />
        public override Expression<Func<Person, bool>> GetMatchExpression()
        {
            return helper.GetMatchExpression((StringIdentifierValue) base.Value);
        }

        /// <inheritdoc />
        public override void Update(Person person)
        {
            helper.Update(person, (StringIdentifierValue) base.Value);
        }

        public const string TypeName = "nhs.uk/nhs-number";
    }
}