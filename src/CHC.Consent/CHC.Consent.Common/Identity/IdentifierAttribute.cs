using System;

namespace CHC.Consent.Common.Identity
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited=false)]
    public class IdentifierAttribute : Attribute
    {
        public string Name { get; }
        public bool AllowMultipleValues { get; set; }

        /// <inheritdoc />
        public IdentifierAttribute(string name)
        {
            Name = name;
        }
    }
}