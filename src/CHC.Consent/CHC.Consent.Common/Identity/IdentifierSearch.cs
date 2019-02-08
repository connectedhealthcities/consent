using System;

namespace CHC.Consent.Common.Identity
{
    public class IdentifierSearch
    {
        public const string Separator = "::";
        public string IdentifierName { get; set; }
        public string Value { get; set; }

        public string KeySelector()
        {
            return IdentifierName.Split(new[] {IdentifierSearch.Separator}, StringSplitOptions.None)[0];
        }
    }
}