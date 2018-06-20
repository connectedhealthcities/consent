namespace CHC.Consent.EFCore.Consent
{
    public abstract class EvidenceEntity : IEntity
    {
        /// <inheritdoc />
        public long Id { get; protected set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public ConsentEntity Consent { get; set; }
    }
    
    public class GivenEvidenceEntity : EvidenceEntity {}
    public class WithdrawnEvidenceEntity : EvidenceEntity {}
}