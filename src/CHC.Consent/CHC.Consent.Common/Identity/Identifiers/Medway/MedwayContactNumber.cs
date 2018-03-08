namespace CHC.Consent.Common.Identity.Identifiers.Medway
{
    [Identifier(TypeName, AllowMultipleValues = true)]
    public class MedwayContactNumber : IPersonIdentifier
    {
        public string Number { get; set; }
        public string Type { get; set; }
        
        public const string TypeName = "uk.nhs.bradfordhospitals.bib4all.medway.contact-number";
    }
}