using System;
using System.Collections.Generic;

namespace CHC.Consent.Common.Infrastructure
{
    public interface ITypeRegistry : IEnumerable<ClassMapping>
    {
        /// <returns>the name of the <paramref name="type"/> or <c>null</c></returns>
        string GetName(Type type);
        /// <returns>the type for the <paramref name="name"/> or <c>null</c></returns>
        Type GetType(string name);

        /// <exception cref="KeyNotFoundException"  />
        Type this[string name] { get; }
        /// <exception cref="KeyNotFoundException"  />
        string this[Type type] { get; }
    }
}