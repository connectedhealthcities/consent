using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CHC.Consent.Common.Infrastructure;

namespace CHC.Consent.Common.Identity.Identifiers
{
    public class IdentifierDefinitionRegistry : 
        KeyedCollection<string, IdentifierDefinition>, 
        IReadOnlyDictionary<string, IdentifierDefinition>
    {
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

        /// <inheritdoc />
        protected override string GetKeyForItem(IdentifierDefinition item) => item.SystemName;

        public IdentifierDefinitionRegistry(params IdentifierDefinition[] definitions) : this(definitions.AsEnumerable())
        {
        }

        public T Accept<T>() where T : IIdentifierDefinitionVisitor, new() => Accept(new T());
        public T Accept<T>(T visitor) where T:IIdentifierDefinitionVisitor
        {
            foreach (var definition in Values)
            {
                definition.Accept(visitor);
            }

            return visitor;
        }

        /// <inheritdoc />
        IEnumerator<KeyValuePair<string, IdentifierDefinition>>
            IEnumerable<KeyValuePair<string, IdentifierDefinition>>.GetEnumerator() => Dictionary.GetEnumerator();

        /// <inheritdoc />
        public bool ContainsKey(string key) => Dictionary.ContainsKey(key);


        /// <inheritdoc />
        public bool TryGetValue(string key, out IdentifierDefinition value) => Dictionary.TryGetValue(key, out value);

        /// <inheritdoc />
        public IEnumerable<string> Keys => Dictionary.Keys;

        /// <inheritdoc />
        public IEnumerable<IdentifierDefinition> Values => Dictionary.Values;
    }
}