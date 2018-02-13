using System;
using System.Linq.Expressions;

namespace CHC.Consent.Common.Identity.Identifiers
{
    [Identifier(TypeName)]
    public class NhsNumberIdentifier : IIdentifier
    {
        private readonly SingleValueIdentifierHelper<string> helper
            = new SingleValueIdentifierHelper<string>(_ => _.NhsNumber);
        /// <inheritdoc />
        public NhsNumberIdentifier(string nhsNumber = null) 
        {
            Value = nhsNumber;
        }

        public string Value { get; set; }


        /// <inheritdoc />
        public Expression<Func<Person, bool>> GetMatchExpression()
            => helper.GetMatchExpression(Value);

        /// <inheritdoc />
        public void Update(Person person) => helper.Update(person, Value);
        

        public const string TypeName = "nhs.uk/nhs-number";
    }
}