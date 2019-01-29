namespace CHC.Consent.EFCore.Consent
{
    public class EvidenceDefinitionEntity : IEntity
    {
        /// <inheritdoc />
        public long Id { get; set; }

        public string Name { get; set; }
        public string Definition { get; set; }
    }
}