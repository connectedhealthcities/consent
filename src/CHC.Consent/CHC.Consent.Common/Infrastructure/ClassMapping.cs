using System;

namespace CHC.Consent.Common.Infrastructure
{
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