using System;
using CHC.Consent.Common.Infrastructure;

namespace CHC.Consent.Common.Consent
{
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