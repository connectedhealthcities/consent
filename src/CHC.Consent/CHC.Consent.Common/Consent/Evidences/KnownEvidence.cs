using System.ComponentModel;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.Common.Infrastructure.Definitions;
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

        public static class WebsiteParts
        {
            public static class UserParts
            {
                public static EvidenceDefinition Name { get; } = String("Name");
                public static EvidenceDefinition Email { get; } = String(("Email"));
                public static EvidenceDefinition Id { get; } = String("Id");
            }

            public static readonly EvidenceDefinition User = Composite("User", UserParts.Email, UserParts.Name);
            public static readonly EvidenceDefinition IpAddress = String("IP Address");

        }

        public static readonly EvidenceDefinition Website = Composite("Website", WebsiteParts.User, WebsiteParts.IpAddress);

        public static class Bib4AllWithdrawalParts
        {
            public static readonly EvidenceDefinition RequestedBy = String("Requested By");
            public static readonly EvidenceDefinition Relationship = String("Relationship to Subject");
        }

        
        
        public static readonly EvidenceDefinition Bib4AllWithdrawal =
            Composite(
                "Bib4All Withdrawal Request",
                Bib4AllWithdrawalParts.RequestedBy,
                Bib4AllWithdrawalParts.Relationship
            );

        private static EvidenceDefinition String(string name)
        {
            return new EvidenceDefinition(name, new StringDefinitionType());
        }

        private static EvidenceDefinition Composite(string name, params IDefinition[] parts)
        {
            return new EvidenceDefinition(name, new CompositeDefinitionType(parts));
        }

        private static EvidenceDefinition Number(string name)
        {
            return new EvidenceDefinition(name, new IntegerDefinitionType());
        }

        public static readonly EvidenceDefinitionRegistry Registry = new EvidenceDefinitionRegistry(ImportFile, Medway, Website, Bib4AllWithdrawal);
    }
}