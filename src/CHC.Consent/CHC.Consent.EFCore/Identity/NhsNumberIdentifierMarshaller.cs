using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore.Identity
{
    public class NhsNumberIdentifierMarshaller : IIdentifierMarshaller<NhsNumberIdentifier>
    {
        public const string ValueTypeName = "string";

        /// <inheritdoc />
        public string ValueType => ValueTypeName;

        /// <inheritdoc />
        public string MarshalledValue(NhsNumberIdentifier value)
        {
            return value.Value;
        }

        /// <inheritdoc />
        public NhsNumberIdentifier Unmarshall(string valueType, string value)
        {
            return valueType == ValueType && value != null ? new NhsNumberIdentifier(value) : null;
        }
    }
}