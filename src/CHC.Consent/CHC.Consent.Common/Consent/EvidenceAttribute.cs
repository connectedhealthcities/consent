using System;
using CHC.Consent.Common.Infrastructure;

namespace CHC.Consent.Common.Consent
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited=false)]
    public class EvidenceAttribute : Attribute, ITypeName
    {
        public EvidenceAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}