namespace CHC.Consent.Common.Identity.Identifiers.Medway
{
    [Identifier("uk.nhs.bradfordhospitals.bib4all.medway.address")]
    public class MedwayAddressIdentifier : IPersonIdentifier
    {
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string AddressLine5 { get; set; }
        public string Postcode { get; set; }
    }
}