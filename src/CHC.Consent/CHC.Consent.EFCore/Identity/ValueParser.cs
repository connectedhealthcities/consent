namespace CHC.Consent.EFCore.Identity
{
    public class ValueParser<T> : IStringValueParser
    {
        public delegate bool TryParseDelegate(string representation, out T value);
        
        public TryParseDelegate Parser { get; }

        /// <inheritdoc />
        public ValueParser(TryParseDelegate parser)
        {
            Parser = parser;
        }

        public bool TryParse(string value, out object result)
        {
            result = default;
            
            if (!Parser(value, out var typedResult)) return false;

            result = typedResult;
            return true;
        }

        public static implicit operator ValueParser<T>(TryParseDelegate parser)
        {
            return new ValueParser<T>(parser);
        }
    }
}