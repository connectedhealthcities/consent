using System.Collections.Generic;
using CHC.Consent.Api.Client.Models;

namespace CHC.Consent.DataTool.Features.ExportData
{
    public class StudySubjectWithIdentifiers : IHaveIdentifiers
    {
        public string subjectIdentifier { get; set; }
        public IList<IIdentifierValueDto> identifiers { get; set; }

        /// <inheritdoc />
        IEnumerable<IIdentifierValueDto> IHaveIdentifiers.Identifiers => identifiers;
    }
}