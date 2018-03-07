using System;
using System.Linq.Expressions;

namespace CHC.Consent.Common.Consent.Identifiers
{
    [ConsentIdentifier("pregnancy-number.consent.bib4all.bradfordhospitals.nhs.uk")]
    public class PregnancyNumberIdentifier : ConsentIdentifier
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