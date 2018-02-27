using System;
using CHC.Consent.Common.Infrastructure;

namespace CHC.Consent.Common.Consent
{
    public class ConsentIdentifierRegistry : TypeRegistry<Identifier, ConsentIdentifierAttribute>
    {
        public virtual void Add<T>() where T : Identifier
        {
            Add(typeof(T));
        }

        public virtual void Add(Type identifierType)
        {
            Add(identifierType, GetIdentifierAttribute(identifierType));
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited=false)]
    public class ConsentIdentifierAttribute : Attribute, ITypeName
    {
        public ConsentIdentifierAttribute(string name)
        {
            Name = name;
        }

        /// <inheritdoc />
        public string Name { get; }
    }
}