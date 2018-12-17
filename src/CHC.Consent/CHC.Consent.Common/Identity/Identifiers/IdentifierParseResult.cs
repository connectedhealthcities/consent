namespace CHC.Consent.Common.Identity.Identifiers
{
    public class IdentifierParseResult
    {
        public bool WasSuccessful { get; }
        public object Value { get;  }

        /// <inheritdoc />
        protected IdentifierParseResult(bool wasSuccessful, object value)
        {
            WasSuccessful = wasSuccessful;
            Value = value;
        }
        
        public static IdentifierParseResult Success(object value) 
            => new IdentifierParseResult(true, value);
        
        public static IdentifierParseResult Failure() 
            => new IdentifierParseResult(false, null);
    }
}