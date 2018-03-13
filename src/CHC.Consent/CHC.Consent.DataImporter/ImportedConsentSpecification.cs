using System;
using System.Text.RegularExpressions;
using CHC.Consent.Api.Client.Models;

namespace CHC.Consent.DataImporter
{
    public class ImportedConsentSpecification
    {
        public DateTime DateGiven { get; set; }
        public long StudyId { get; set; }

        public MatchSpecification[] GivenBy { get; set; } = Array.Empty<MatchSpecification>();

        public Evidence[] Evidence { get; set; } = Array.Empty<Evidence>();
        public ConsentIdentifier[] CaseId { get; set; } = Array.Empty<ConsentIdentifier>();
    }
}