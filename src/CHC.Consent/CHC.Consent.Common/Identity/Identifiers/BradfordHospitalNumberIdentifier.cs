using System.Linq;

namespace CHC.Consent.Common.Identity.Identifiers
{
    [Identifier(TypeName, AllowMultipleValues = true)]
    public class BradfordHospitalNumberIdentifier : IPersonIdentifier
    {
        public string Value { get; private set; }
        
        public BradfordHospitalNumberIdentifier(string value=null)
        {
            Value = value;
        }

        public const string TypeName = "uk.nhs.bradfordhospitals.hosptial-id";
    }
}