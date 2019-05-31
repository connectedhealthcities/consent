namespace CHC.Consent.EFCore.Identity
{
    public class PersonAgencyId : IEntity
    {
        public long Id { get; set; }
        public long PersonId { get; set; }
        public long AgencyId { get; set; }
        public string SpecificId { get; set; }
    }
}