namespace CHC.Consent.Api.Features.Identity.Dto
{
    public class IdentifierSpecification
    {
        /// <remarks>this maps to <see cref="IdentifierType.ExternalId"/></remarks>>
        public string Type { get; set; }
        /// <summary>
        /// The Id used for reference by <see cref="MatchIdentifierSpecification"/>
        /// </summary>
        public string Id { get; set; }
        public string Value { get; set; }
    }
}