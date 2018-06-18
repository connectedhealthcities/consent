namespace CHC.Consent.Common.Identity.Identifiers.Medway
{
    [Identifier("uk.nhs.bradfordhospitals.bib4all.medway.birth-order", DisplayName = "Birth Order")]
    public class MedwayBirthOrder : IPersonIdentifier
    {
        public int PregnancyNumber { get; set; }
        public int BirthOrder { get; set; }
    }
}