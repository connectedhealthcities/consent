using System;
using System.Collections.Generic;

namespace CHC.Consent.Common.Infrastructure
{
    public interface ITypeRegistry : IEnumerable<ClassMapping>
    {
        bool TryGetName(Type type, out string name);
        bool TryGetType(string name, out  Type type);

        /// <exception cref="KeyNotFoundException"  />
        Type this[string name] { get; }
        /// <exception cref="KeyNotFoundException"  />
        string this[Type type] { get; }
    }
}