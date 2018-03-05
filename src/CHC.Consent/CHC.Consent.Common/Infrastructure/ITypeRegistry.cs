using System;
using System.Collections.Generic;

namespace CHC.Consent.Common.Infrastructure
{
    /// <summary>
    /// A mapping between types and names - used for marhsalling/serializations 
    /// </summary>
    public interface ITypeRegistry : IEnumerable<ClassMapping>
    {
        bool TryGetName(Type type, out string name);
        bool TryGetType(string name, out  Type type);

        /// <exception cref="KeyNotFoundException"  />
        Type this[string name] { get; }
        /// <exception cref="KeyNotFoundException"  />
        string this[Type type] { get; }
    }

    /// <summary>
    /// A Marker interface for mappings between types inheritifed from a base type/interfce
    /// Mostly useful for DI/IOC use
    /// </summary>
    /// <typeparam name="TBaseType">The base type from which all types must inherit, or interface they must all implement</typeparam>
    public interface ITypeRegistry<TBaseType> : ITypeRegistry
    {

    }

}