using System;
using CHC.Consent.Common.Infrastructure;

namespace CHC.Consent.Common.Consent
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited=false)]
    public class CaseIdentifierAttribute : Attribute, ITypeName
    {
        public CaseIdentifierAttribute(string name)
        {
            Name = name;
        }

        /// <inheritdoc />
        public string Name { get; }
    }
}