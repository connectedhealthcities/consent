namespace CHC.Consent.Common.Consent.Identifiers
{
    [CaseIdentifier(TypeName)]
    public class PregnancyNumberIdentifier : CaseIdentifier
    {
        public const string TypeName = "uk.nhs.bradfordhospitals.bib4all.consent.pregnancy-number";
        public string Value { get; set; }

        /// <inheritdoc />
        protected PregnancyNumberIdentifier()
        {
        }

        /// <inheritdoc />
        public PregnancyNumberIdentifier(string value = null)
        {
            Value = value;
        }
    }
}