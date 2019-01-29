using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure.Definitions.Types;

namespace CHC.Consent.Common.Consent.Evidences
{
    public static class KnownEvidence
    {
        public static class ImportFileParts
        {
            public static EvidenceDefinition BaseUri { get; } = new EvidenceDefinition("Base Uri", new StringDefinitionType());
            public static EvidenceDefinition LineNumber { get; } = new EvidenceDefinition("Line Number", new IntegerDefinitionType());
            public static EvidenceDefinition LinePosition { get; } = new EvidenceDefinition("Line Position", new IntegerDefinitionType());
        }
        
        public static EvidenceDefinition ImportFile { get; } = new EvidenceDefinition("Import File", new CompositeDefinitionType(
            ImportFileParts.BaseUri,
            ImportFileParts.LineNumber,
            ImportFileParts.LinePosition
            ));

        public static class MedwayParts
        {
            public static EvidenceDefinition CompetentStatus { get; } = new EvidenceDefinition("Competent Status", new StringDefinitionType());
            public static EvidenceDefinition ConsentGivenBy { get; } = new EvidenceDefinition("Consent Given By", new StringDefinitionType());
            public static EvidenceDefinition ConsentTakenBy { get; } = new EvidenceDefinition("Consent Taken By", new StringDefinitionType());
        }

        public static EvidenceDefinition Medway { get; } = new EvidenceDefinition(
            "Medway",
            new CompositeDefinitionType(
                MedwayParts.CompetentStatus,
                MedwayParts.ConsentGivenBy,
                MedwayParts.ConsentTakenBy
            ));

        public static EvidenceDefinitionRegistry Registry = new EvidenceDefinitionRegistry(ImportFile, Medway);
    }
}