using System.Collections.Generic;
using CHC.Consent.Api.Client.Models;

namespace CHC.Consent.DataImporter.Features.ExportData
{
    public interface IHaveIdentifiers
    {
        IEnumerable<IIdentifierValueDto> Identifiers { get; }
    }
}