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
                public static StringEvidenceDefinition Name { get; } = String("Name");
                public static StringEvidenceDefinition Email { get; } = String(("Email"));
                public static StringEvidenceDefinition Id { get; } = String("Id");
            }

            public static readonly CompositeEvidenceDefinition User = Composite("User", UserParts.Email, UserParts.Name);
            public static readonly StringEvidenceDefinition IpAddress = String("IP Address");

        }

        public static readonly CompositeEvidenceDefinition Website = Composite("Website", WebsiteParts.User, WebsiteParts.IpAddress);

        public static class Bib4AllWithdrawalParts
        {
            public static readonly StringEvidenceDefinition RequestedBy = String("Requested By");
            public static readonly StringEvidenceDefinition Relationship = String("Relationship to Subject");
        }

        public class CompositeEvidenceDefinition : EvidenceDefinition
        {
            /// <inheritdoc />
            public CompositeEvidenceDefinition(string name, IDefinitionType type) : base(name, type)
            {
            }

            public Evidence Create(params Evidence[] parts)
            {
                return Evidence.Composite(this, parts);
            }
        }

        public class StringEvidenceDefinition : EvidenceDefinition
        {
            /// <inheritdoc />
            public StringEvidenceDefinition(string name, IDefinitionType type) : base(name, type)
            {
            }

            public Evidence Create(string value)
            {
                return Evidence.String(this, value);
            }
        }
        
        public static readonly CompositeEvidenceDefinition Bib4AllWithdrawal =
            Composite(
                "Bib4All Withdrawal Request",
                Bib4AllWithdrawalParts.RequestedBy,
                Bib4AllWithdrawalParts.Relationship
            );

        private static StringEvidenceDefinition String(string name)
        {
            return new StringEvidenceDefinition(name, new StringDefinitionType());
        }

        private static CompositeEvidenceDefinition Composite(string name, params IDefinition[] parts)
        {
            return new CompositeEvidenceDefinition(name, new CompositeDefinitionType(parts));
        }

        private static EvidenceDefinition Number(string name)
        {
            return new EvidenceDefinition(name, new IntegerDefinitionType());
        }

        public static readonly EvidenceDefinitionRegistry Registry = new EvidenceDefinitionRegistry(ImportFile, Medway, Website, Bib4AllWithdrawal);
    }
}