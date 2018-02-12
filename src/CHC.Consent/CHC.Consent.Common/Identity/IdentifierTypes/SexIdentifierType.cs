using System.Runtime.InteropServices;
using CHC.Consent.Common.Identity.IdentifierValues;

namespace CHC.Consent.Common.Identity.IdentifierTypes
{
    public class SexIdentifierType : SingleValueIdentifierType<SexIdentifierValue, SexIdentifierValueType, Sex?>
    {
        /// <inheritdoc />
        public SexIdentifierType() : base(TypeName, _ => _.Sex)
        {
        }

        public const string TypeName = "sex";
    }
}