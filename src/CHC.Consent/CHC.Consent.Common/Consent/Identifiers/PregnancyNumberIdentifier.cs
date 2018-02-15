using System;
using System.Linq.Expressions;

namespace CHC.Consent.Common.Consent.Identifiers
{
    public class PregnancyNumberIdentifier : Identifier
    {
        public string Value { get; }

        /// <inheritdoc />
        public PregnancyNumberIdentifier(string value = null)
        {
            Value = value;
        }

        /// <inheritdoc />
        public override void Update(Consent consent)
        {
            consent.PregnancyNumber = Value;
        }

        public override Expression<Func<Consent,bool>> CreateMatchIdentifier()
        {
            return consent => consent.PregnancyNumber == Value;
        }
    }
}