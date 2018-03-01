using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CHC.Consent.Common.Identity;

namespace CHC.Consent.Common.Infrastructure
{
    public static class IdentifierAttributeHelpers
    {
        public static TAttribute GetIdentiferAttribute<TAttribute>(Type identifierType)  where TAttribute: Attribute, ITypeName
        {
            var identifierAttribute = identifierType.GetCustomAttribute<TAttribute>();
            if (identifierAttribute == null)
            {
                throw new ArgumentException(
                    $"Cannot get attributes for {identifierType} as it has no {typeof(TAttribute).Name}");
            }

            return identifierAttribute;
        }
        
    }

    public interface ITypeRegistry<TIdentifier> : ITypeRegistry
    {
        
    }
    
    public class TypeRegistry<TIdentifier, TAttribute> : ITypeRegistry<TIdentifier> where TAttribute: Attribute, ITypeName
    {
        private readonly IDictionary<string, Type> namesToTypes = new Dictionary<string, Type>();

        private readonly IDictionary<Type, string> typesToNames = new Dictionary<Type, string>();


        public static TAttribute GetIdentifierAttribute(Type identifierType) =>
            IdentifierAttributeHelpers.GetIdentiferAttribute<TAttribute>(identifierType);


        public virtual void Add(Type identifierType, TAttribute attribute)
        {
            if(!typeof(TIdentifier).IsAssignableFrom(identifierType))
                throw new ArgumentException();
            Add(identifierType, attribute.Name);
        }

        public void Add(Type identifierType, string name)
        {
            namesToTypes.Add(name, identifierType);
            typesToNames.Add(identifierType, name);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<ClassMapping> GetEnumerator()
        {
            return namesToTypes.Select(_ => new ClassMapping(name:_.Key, type:_.Value)).GetEnumerator();
        }
        
        public bool TryGetName(Type type, out string name) => typesToNames.TryGetValue(type, out name);

        public bool TryGetType(string name, out Type type) => namesToTypes.TryGetValue(name, out type);
        

        public Type this[string name] => namesToTypes[name];
        public string this[Type type] => typesToNames[type];
    }
}