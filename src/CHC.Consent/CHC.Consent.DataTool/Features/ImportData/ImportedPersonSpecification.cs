using CHC.Consent.Api.Client.Models;

namespace CHC.Consent.DataTool.Features.ImportData
{
    public class ImportedPersonSpecification
    {
        public PersonSpecification PersonSpecification { get; set; }
        public ImportedConsentSpecification[] ConsentSpecifications { get; set; }
    }
}
