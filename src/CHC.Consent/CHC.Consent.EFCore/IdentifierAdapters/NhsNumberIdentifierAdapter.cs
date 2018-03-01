using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore.IdentifierAdapters
{
    public class NhsNumberIdentifierMarshaller : IIdentifierMarshaller<NhsNumberIdentifier>
    {
        /// <inheritdoc />
        public string ValueType => "string";

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
    
    public class NhsNumberIdentifierAdapter : IdentifierAdapterBase<NhsNumberIdentifier>
    {
        /// <inheritdoc />
        public NhsNumberIdentifierAdapter() : base(new NhsNumberIdentifierMarshaller(), NhsNumberIdentifier.TypeName)
        {
        }
    }
}