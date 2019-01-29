namespace CHC.Consent.Common.Identity.Identifiers
{
    public class IdentifierDefinition : DefinitionBase
    {
        public IdentifierDefinition(string name, IIdentifierType type) : base(name, type)
        {
        }

        public static IdentifierDefinition Create(string name, IIdentifierType type)
            => new IdentifierDefinition(name, type);
            
        
    }
}