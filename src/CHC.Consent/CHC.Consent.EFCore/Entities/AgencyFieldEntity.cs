using CHC.Consent.EFCore.Identity;
using JetBrains.Annotations;

namespace CHC.Consent.EFCore.Entities
{
    [UsedImplicitly]
    public class AgencyFieldEntity : IEntity
    {
        public long Id { get; set; }
        public AgencyEntity Agency { get; set; }
        public IdentifierDefinitionEntity Identifier { get; set; }
        public string Subfields { get; set; }
        public int Order { get; set; }
    }
}