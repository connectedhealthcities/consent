using System;

namespace CHC.Consent.Common.Infrastructure
{
    /// <summary>
    /// Presents a mapping between a <see cref="Type"/> and a <see cref="String"/> name
    /// </summary>
    public class ClassMapping
    {
        public string Name { get; }
        public Type Type { get;  }

        /// <inheritdoc />
        public ClassMapping(string name, Type type)
        {
            Name = name;
            Type = type;
        }
    }
}