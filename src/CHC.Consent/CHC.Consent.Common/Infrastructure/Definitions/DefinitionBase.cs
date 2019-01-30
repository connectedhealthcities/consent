namespace CHC.Consent.Common.Infrastructure.Definitions
{
    public abstract class DefinitionBase : IDefinition
    {
        protected DefinitionBase(string name, IDefinitionType type)
        {
            Name = name;
            Type = type;
            SystemName = MakeSystemName(Name);
            AsString = SystemName + ':' + Type.SystemName;
        }

        public string Name { get; }
        public string SystemName { get; }
        public IDefinitionType Type { get; }
        private static string MakeSystemName(string name) => name.Replace(" ", "-").ToLowerInvariant();
        public string AsString { get; }
    }
}