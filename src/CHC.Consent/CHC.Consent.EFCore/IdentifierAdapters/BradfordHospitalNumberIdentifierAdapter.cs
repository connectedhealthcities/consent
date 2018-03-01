using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore.IdentifierAdapters
{
    public class BradfordHospitalNumberIdentifierAdapter : IdentifierAdapterBase<BradfordHospitalNumberIdentifier>
    {
        /// <inheritdoc />
        public BradfordHospitalNumberIdentifierAdapter() : 
            base(new BradfordHospitalNumberIdentifierMarshaller(), BradfordHospitalNumberIdentifier.TypeName)
        {
        }
    }

    public class BradfordHospitalNumberIdentifierMarshaller : IIdentifierMarshaller<BradfordHospitalNumberIdentifier>
    {
        /// <inheritdoc />
        public string ValueType => "string";

        /// <inheritdoc />
        public string MarshalledValue(BradfordHospitalNumberIdentifier value)
        {
            return value.Value;
        }

        /// <inheritdoc />
        public BradfordHospitalNumberIdentifier Unmarshall(string valueType, string value)
        {
            return valueType == ValueType && value != null ? new BradfordHospitalNumberIdentifier(value) : null;
        }
    }
}