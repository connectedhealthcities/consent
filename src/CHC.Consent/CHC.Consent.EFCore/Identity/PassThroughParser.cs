namespace CHC.Consent.EFCore.Identity
{
    public class PassThroughParser : IStringValueParser
    {
        public static IStringValueParser Instance { get; } = new PassThroughParser();
        /// <inheritdoc />
        private PassThroughParser()
        {
        }

        /// <inheritdoc />
        public bool TryParse(string value, out object result)
        {
            result = value;
            return true;
        }
    }
}