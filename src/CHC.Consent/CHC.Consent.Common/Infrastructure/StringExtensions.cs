using System.Text.RegularExpressions;

namespace CHC.Consent.Common.Infrastructure
{
    public static class StringExtensions
    {
        private static readonly Regex LowerCaseFollowedByUpperCase = new Regex(
            @"(\p{Ll})\W?(\p{Lu})",
            RegexOptions.Compiled);
        
        public static string ToKebabCase(this string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            return Regex.Replace(
                    name,
                    "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z0-9])",
                    "-$1",
                    RegexOptions.Compiled)
                .Trim()
                .ToLower();
        }
    }
}