using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore.Identity
{
    public class BradfordHospitalNumberIdentifierMarshaller : IIdentifierMarshaller<BradfordHospitalNumberIdentifier>
    {
        public const string ValueTypeName = "string";

        /// <inheritdoc />
        public string ValueType => ValueTypeName;

        /// <inheritdoc />
        public string MarshalledValue(BradfordHospitalNumberIdentifier value)
        {
            return value.Value;
        }

        /// <inheritdoc />
        public BradfordHospitalNumberIdentifier Unmarshall(string valueType, string value)
        {
            return valueType == ValueTypeName && value != null ? new BradfordHospitalNumberIdentifier(value) : null;
        }
    }
}