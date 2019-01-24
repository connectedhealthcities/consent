using System.Collections.Generic;
using CHC.Consent.Api.Client.Models;

namespace CHC.Consent.DataImporter.Features.ExportData
{
    public class StudySubjectWithIdentifiers
    {
        public string subjectIdentifier { get; set; }
        public IList<IIdentifierValueDto> identifiers { get; set; }
    }
}