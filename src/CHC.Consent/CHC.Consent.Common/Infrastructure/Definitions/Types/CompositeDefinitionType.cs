using System.Linq;

namespace CHC.Consent.Common.Infrastructure.Definitions.Types
{
    public class CompositeDefinitionType : IDefinitionType 
    {
        public DefinitionRegistry Identifiers { get; }

        /// <inheritdoc />
        public void Accept(IDefinitionVisitor visitor, IDefinition definition)
        {
            visitor.Visit(definition, this);
        }

        public string SystemName { get; }

        public CompositeDefinitionType(params IDefinition[] identifiers) 
        {
            Identifiers = new DefinitionRegistry(identifiers);   
            SystemName = $"composite({string.Join(",", identifiers.Select(_ => _.AsString))})";
        }
    }
}