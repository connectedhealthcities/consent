using CHC.Consent.Common.Identity.Identifiers.Medway;
using CHC.Consent.Common.Infrastructure.Data;

namespace CHC.Consent.EFCore.Entities
{
    public class MedwayNameEntity : MedwayNameIdentifier, IEntity
    {
        public long Id { get; set; }
        public PersonEntity Person { get; set; }
    }
}