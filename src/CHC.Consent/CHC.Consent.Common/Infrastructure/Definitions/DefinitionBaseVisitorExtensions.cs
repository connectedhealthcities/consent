namespace CHC.Consent.Common.Infrastructure.Definitions
{
    public static class DefinitionBaseVisitorExtensions
    {
        public static void Accept(this IDefinition definition, IDefinitionVisitor visitor)
            => definition.Type.Accept(visitor, definition);
        public static void Accept<T>(this T definition, IDefinitionVisitor visitor) where T:DefinitionBase
            => definition.Type.Accept(visitor, definition);
    }
}