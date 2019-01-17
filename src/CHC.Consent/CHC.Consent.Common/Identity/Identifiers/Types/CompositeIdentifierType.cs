using System.Collections.Generic;
using System.Linq;

namespace CHC.Consent.Common.Identity.Identifiers
{
    public class CompositeIdentifierType : IIdentifierType 
    {
        public DefinitionRegistry Identifiers { get; }

        /// <inheritdoc />
        public void Accept(IDefinitionVisitor visitor, IDefinition definition)
        {
            visitor.Visit(definition, this);
        }

        public string SystemName { get; }

        public CompositeIdentifierType(params IDefinition[] identifiers) 
        {
            Identifiers = new DefinitionRegistry(identifiers);   
            SystemName = $"composite({string.Join(",", identifiers.Select(_ => _.Type.SystemName))})";
        }
    }
}