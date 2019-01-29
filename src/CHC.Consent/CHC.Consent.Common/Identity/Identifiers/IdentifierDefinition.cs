using CHC.Consent.Common.Infrastructure.Definitions;

namespace CHC.Consent.Common.Identity.Identifiers
{
    public class IdentifierDefinition : DefinitionBase
    {
        public IdentifierDefinition(string name, IDefinitionType type) : base(name, type)
        {
        }

        public static IdentifierDefinition Create(string name, IDefinitionType type)
            => new IdentifierDefinition(name, type);
            
        
    }
}