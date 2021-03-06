using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CHC.Consent.Common.Infrastructure.Definitions
{
    public class DefinitionRegistry : KeyedCollection<string, IDefinition>
    {
        /// <inheritdoc />
        protected DefinitionRegistry()
        {
        }

        public DefinitionRegistry(IEnumerable<IDefinition> definitions)
        {
            foreach (var definition in definitions)
            {
                Add(definition);
            }
        }

        /// <inheritdoc />
        protected override string GetKeyForItem(IDefinition item) => item.SystemName;

        /// <inheritdoc />
        public bool ContainsKey(string key) => Contains(key);


        /// <inheritdoc />
        public bool TryGetValue(string key, out IDefinition value) => Dictionary.TryGetValue(key, out value);

        public TVisitor Accept<TVisitor>(TVisitor visitor) where TVisitor:IDefinitionVisitor
        {
            foreach (var definition in this)
            {
                definition.Accept(visitor);
            }

            return visitor;
        }
    }
}