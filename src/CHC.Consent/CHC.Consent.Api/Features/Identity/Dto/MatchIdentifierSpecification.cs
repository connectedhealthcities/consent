namespace CHC.Consent.Api.Features.Identity.Dto
{
    public class MatchIdentifierSpecification
    {
        public MatchBy MatchBy { get; set; }
        public string IdOrType { get; set; }
    }
}