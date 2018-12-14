using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Infrastructure;

namespace CHC.Consent.Common.Identity.Identifiers
{
    public class IdentifierDefinitionRegistry : IReadOnlyDictionary<string, IdentifierDefinition>
    {
        private Dictionary<string, IdentifierDefinition> Definitions { get; } =
            new Dictionary<string, IdentifierDefinition>();
        
        /// <inheritdoc />
        public IdentifierDefinitionRegistry()
        {
        }

        /// <inheritdoc />
        public IdentifierDefinitionRegistry(IEnumerable<IdentifierDefinition> definitions) 
        {
            foreach (var definition in definitions)
            {
                Add(definition);
            }
        }
        
        public IdentifierDefinitionRegistry(params IdentifierDefinition[] definitions) : this(definitions.AsEnumerable())
        {
        }

        public bool Add(IdentifierDefinition definition)
        {
            var systemName = definition.SystemName;
            
            if (Definitions.ContainsKey(systemName)) return false;
            Definitions.Add(systemName, definition);
            return true;
        }


        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, IdentifierDefinition>> GetEnumerator() => Definitions.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public int Count => Definitions.Count;

        /// <inheritdoc />
        public bool ContainsKey(string key) => Definitions.ContainsKey(key);
        

        /// <inheritdoc />
        public bool TryGetValue(string key, out IdentifierDefinition value)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IdentifierDefinition this[string key]
        {
            get => Definitions[key];
            set => Definitions[IdentifierDefinition.MakeSystemName(key)] = value;
        }

        /// <inheritdoc />
        public IEnumerable<string> Keys => Definitions.Keys;

        /// <inheritdoc />
        public IEnumerable<IdentifierDefinition> Values => Definitions.Values;

        public void Accept(IIdentifierDefinitionVisitor visitor)
        {
            foreach (var definition in Values)
            {
                definition.Accept(visitor);
            }
        }
    }
}