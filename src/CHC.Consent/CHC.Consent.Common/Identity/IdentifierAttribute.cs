using System;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Infrastructure;

namespace CHC.Consent.Common.Identity
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited=false)]
    public class IdentifierAttribute : Attribute, ITypeName
    {
        public string Name { get; }
        public string DisplayName { get; set; }
        public bool AllowMultipleValues { get; set; }

        /// <inheritdoc />
        public IdentifierAttribute(string name)
        {
            Name = name;
        }

        public static IdentifierAttribute GetAttribute(Type identifierType)
            => TypeNameHelpers.GetIdentiferAttribute<IdentifierAttribute>(identifierType);
    }
}