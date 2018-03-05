using System;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.EFCore.Identity;

namespace CHC.Consent.EFCore.Identity
{
    public class SexIdentifierMarshaller : IIdentifierMarshaller<SexIdentifier>
    {
        public const string ValueTypeName = "sex";
        public string ValueType => ValueTypeName;

        public string MarshalledValue(SexIdentifier value)
        {
            return value.Sex?.ToString();
        }

        public SexIdentifier Unmarshall(string valueType, string value)
        {
            return valueType == ValueType && Enum.TryParse<Sex>(value, out var sex)  ? new SexIdentifier(sex) : null;
        }
    }
}