namespace CHC.Consent.Common.Identity.Identifiers
{
    public interface IDefinition
    {
        string SystemName { get; }
        IIdentifierType Type { get; }
    }

    public abstract class DefinitionBase : IDefinition
    {
        protected DefinitionBase(string name, IIdentifierType type)
        {
            Name = name;
            Type = type;
            SystemName = MakeSystemName(Name);
        }

        public string Name { get; }
        public string SystemName { get; }
        public IIdentifierType Type { get; }
        private static string MakeSystemName(string name) => name.Replace(" ", "-").ToLowerInvariant();
    }

    public static class DefinitionBaseVisitorExtensions
    {
        public static void Accept(this IDefinition definition, IDefinitionVisitor visitor)
            => definition.Type.Accept(visitor, definition);
        public static void Accept<T>(this T definition, IDefinitionVisitor visitor) where T:DefinitionBase
            => definition.Type.Accept(visitor, definition);
    }
}