namespace CHC.Consent.Common.Identity.Identifiers
{
    public class IdentifierDefinition
    {
        public string Name { get; }
        public string SystemName { get; }
        public IIdentifierType Type { get; }

        public IdentifierDefinition(string name, IIdentifierType type)
        {
            Name = name;
            Type = type;
            SystemName = MakeSystemName(Name);
        }

        public static string MakeSystemName(string name) => name.Replace(" ", "-").ToLowerInvariant();

        public void Accept(IIdentifierDefinitionVisitor visitor)
        {
            Type.Accept(visitor, this);
        }
    }
}