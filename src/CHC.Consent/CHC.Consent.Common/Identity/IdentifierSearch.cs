using System;

namespace CHC.Consent.Common.Identity
{
    public class IdentifierSearch
    {
        public const string Separator = "::";
        public string IdentifierName { get; set; }
        public string Value { get; set; }

        public IdentifierSearchOperator Operator { get; set; } = IdentifierSearchOperator.Contains;

        public string KeySelector()
        {
            return RootIdentifier(IdentifierName);
        }

        public static string RootIdentifier(string identifierName)
        {
            return identifierName.Split(new[] {Separator}, StringSplitOptions.None)[0];
        }
    }
}