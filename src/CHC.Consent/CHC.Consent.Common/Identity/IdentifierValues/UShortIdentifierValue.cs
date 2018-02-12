namespace CHC.Consent.Common.Identity.IdentifierValues
{
    public class UShortIdentifierValue : IdentifierValue, IIdentifierValue<ushort?>
    {
        public UShortIdentifierValue(ushort value)
        {
            Value = value;
        }

        /// <inheritdoc />
        public ushort Value { get; }

        /// <inheritdoc />
        ushort? IIdentifierValue<ushort?>.Value => Value;
    }
}