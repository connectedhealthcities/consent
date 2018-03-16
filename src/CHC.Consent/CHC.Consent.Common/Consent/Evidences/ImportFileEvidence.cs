using JetBrains.Annotations;

namespace CHC.Consent.Common.Consent.Evidences
{
    [UsedImplicitly]
    [Evidence("org.connectedhealthcities.import.file-source")]
    public class ImportFileEvidence : Evidence
    {
        public string BaseUri { get; set; }
        public long? LineNumber { get; set; }
        public long? LinePosition { get; set; }
    }
}