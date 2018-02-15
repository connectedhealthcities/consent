namespace CHC.Consent.Common.Consent.Identifiers
{
    public class PregnancyIdIdentifier : Identifier
    {
        public string Value { get; }

        /// <inheritdoc />
        public PregnancyIdIdentifier(string value = null)
        {
            Value = value;
        }

        /// <inheritdoc />
        public override void Update(Consent consent)
        {
            consent.PregnancyNumber = Value;
        }
    }
}