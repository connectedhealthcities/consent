using System.Linq;

namespace CHC.Consent.Common.Identity.Identifiers
{
    public class CompositeIdentifierType : IIdentifierType
    {
        public IdentifierDefinitionRegistry Identifiers { get; }

        public CompositeIdentifierType(params IdentifierDefinition[] identifiers)
        {
            Identifiers = new IdentifierDefinitionRegistry(identifiers);
            SystemName = $"composite({string.Join(",", identifiers.Select(_ => _.Type.SystemName))})";
        }

        /// <inheritdoc />
        public void Accept(IIdentifierDefinitionVisitor visitor, IdentifierDefinition definition)
        {
            visitor.Visit(definition, this);
        }

        /// <inheritdoc />
        public string SystemName { get; }
    }
}