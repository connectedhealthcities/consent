using CHC.Consent.Common.Infrastructure.Definitions;

namespace CHC.Consent.Testing.Utils
{
    public static class DefinitionExtensions
    {
        public static string ToDefinition(this IDefinition definition)
        {
            var visitor = new IdentifierDefinitionCreator();
            definition.Accept(visitor);
            return visitor.DefinitionText;
        }
    }
}