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
    }
}