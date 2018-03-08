using System;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Identity.Identifiers.Medway;
using CHC.Consent.EFCore.Identity;

namespace CHC.Consent.EFCore.Identity
{
    public class SexIdentifierMarshaller : IIdentifierMarshaller<MedwaySexIdentifier>
    {
        public const string ValueTypeName = "sex";
        public string ValueType => ValueTypeName;

        public string MarshalledValue(MedwaySexIdentifier value)
        {
            return value.Sex?.ToString();
        }

        public MedwaySexIdentifier Unmarshall(string valueType, string value)
        {
            return valueType == ValueType && Enum.TryParse<Sex>(value, out var sex)  ? new MedwaySexIdentifier(sex) : null;
        }
    }
}