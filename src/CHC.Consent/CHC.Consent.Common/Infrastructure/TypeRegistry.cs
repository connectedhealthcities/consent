using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Identity;

namespace CHC.Consent.Common.Infrastructure
{
    /// <summary>
    /// <para>
    /// An implmentation of a <see cref="ITypeRegistry{TIdentifier}"/>
    /// which uses <typeparamref name="TAttribute"/> to provide names
    /// </para>
    /// </summary>
    /// <typeparam name="TBaseType">The base type/interface which types inherit/implement</typeparam>
    /// <typeparam name="TAttribute">The type of the attribute - must implement <see cref="ITypeName"/></typeparam>
    public class TypeRegistry<TBaseType, TAttribute> : ITypeRegistry<TBaseType> 
        where TAttribute: Attribute, ITypeName
    {
        private readonly IDictionary<string, Type> namesToTypes = new Dictionary<string, Type>();
        private readonly IDictionary<Type, string> typesToNames = new Dictionary<Type, string>();

        protected static TAttribute GetIdentifierAttribute(Type identifierType) =>
            TypeNameHelpers.GetIdentiferAttribute<TAttribute>(identifierType);

        public virtual void Add<T>() where T:TBaseType
            => Add(typeof(T), GetIdentifierAttribute(typeof(T)));
        

        public virtual void Add(Type type, TAttribute attribute)
        {
            Add(type, attribute.Name);
        }

        public void Add(Type type, string name)
        {
            if(!type.IsSubtypeOf<TBaseType>()) 
                throw new ArgumentException($"{type} is not a {typeof(TBaseType)}", nameof(type));
            namesToTypes.Add(name, type);
            typesToNames.Add(type, name);
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