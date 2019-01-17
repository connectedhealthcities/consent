using CHC.Consent.Common.Identity.Identifiers;

namespace CHC.Consent.Common.Consent.Evidences
{
    public static class KnownEvidence
    {
        public static class ImportFileParts
        {
            public static EvidenceDefinition BaseUri { get; } = new EvidenceDefinition("Base Uri", new StringIdentifierType());
            public static EvidenceDefinition LineNumber { get; } = new EvidenceDefinition("Line Number", new IntegerIdentifierType());
            public static EvidenceDefinition LinePosition { get; } = new EvidenceDefinition("Line Position", new IntegerIdentifierType());
        }
        
        public static EvidenceDefinition ImportFile { get; } = new EvidenceDefinition("Import File", new CompositeIdentifierType(
            ImportFileParts.BaseUri,
            ImportFileParts.LineNumber,
            ImportFileParts.LinePosition
            ));

        public static class MedwayParts
        {
            public static EvidenceDefinition CompetentStatus { get; } = new EvidenceDefinition("Competent Status", new StringIdentifierType());
            public static EvidenceDefinition ConsentGivenBy { get; } = new EvidenceDefinition("Consent Given By", new StringIdentifierType());
            public static EvidenceDefinition ConsentTakenBy { get; } = new EvidenceDefinition("Consent Taken By", new StringIdentifierType());
        }

        public static EvidenceDefinition Medway { get; } = new EvidenceDefinition(
            "Medway",
            new CompositeIdentifierType(
                MedwayParts.CompetentStatus,
                MedwayParts.ConsentGivenBy,
                MedwayParts.ConsentTakenBy
            ));

        public static EvidenceDefinitionRegistry Registry = new EvidenceDefinitionRegistry(ImportFile, Medway);
    }
}