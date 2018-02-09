using CHC.Consent.Common.Identity.IdentifierTypes;

namespace CHC.Consent.Common.Identity.IdentifierValues
{
    public class SexIdentifierValue : IdentifierValue, IIdentifierValue<Sex?>
    {
        /// <inheritdoc />
        public SexIdentifierValue(Common.Sex sex)
        {
            Sex = sex;
        }

        public Sex Sex { get; }
        
        /// <inheritdoc />
        Sex? IIdentifierValue<Sex?>.Value => this.Sex;
    }
}