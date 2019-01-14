namespace CHC.Consent.EFCore.Consent
{
    public class CaseIdentifierEntity : IEntity
    {
        /// <inheritdoc />
        public long Id { get; protected set; }
        
        public string Value { get; set; }
        public string Type { get; set; }
        public ConsentEntity Consent { get; set; }
    }
}