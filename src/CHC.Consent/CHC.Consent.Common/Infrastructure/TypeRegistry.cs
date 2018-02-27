using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CHC.Consent.Common.Identity;

namespace CHC.Consent.Common.Infrastructure
{
    public abstract class TypeRegistry<TIdentifierBase, TAttribute> : ITypeRegistry where TAttribute: Attribute, ITypeName
    {
        private readonly IDictionary<string, Type> namesToTypes = new Dictionary<string, Type>();

        private readonly IDictionary<Type, string> typesToNames = new Dictionary<Type, string>();


        protected static TAttribute GetIdentifierAttribute(Type identifierType)
        {
            var identifierAttribute = identifierType.GetCustomAttribute<TAttribute>();
            if (identifierAttribute == null)
            {
                throw new ArgumentException(
                    $"Cannot get attributes for {identifierType} as it has no {typeof(TAttribute).Name}");
            }

            return identifierAttribute;
        }

        protected virtual void Add(Type identifierType, TAttribute attribute)
        {
            namesToTypes.Add(attribute.Name, identifierType);
            typesToNames.Add(identifierType, attribute.Name);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<ClassMapping> GetEnumerator()
        {
            return namesToTypes.Select(_ => new ClassMapping(name:_.Key, type:_.Value)).GetEnumerator();
        }
        
        public string GetName(Type type)
        {
            return typesToNames.TryGetValue(type, out var name) ? name : null;
        }

        /// <inheritdoc />
        public Type GetType(string name)
        {
            return namesToTypes.TryGetValue(name, out var type) ? type : null;
        }

        public Type this[string name] => namesToTypes[name];
        public string this[Type type] => typesToNames[type];
    }
}