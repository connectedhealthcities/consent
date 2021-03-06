using System;
using CHC.Consent.Api.Client.Models;

namespace CHC.Consent.DataTool.Features.ImportData
{
    public class ImportedConsentSpecification
    {
        public DateTime DateGiven { get; set; }
        public long StudyId { get; set; }

        public MatchSpecification[] GivenBy { get; set; } = Array.Empty<MatchSpecification>();

        public IIdentifierValueDto[] Evidence { get; set; } = Array.Empty<IIdentifierValueDto>();
    }
}