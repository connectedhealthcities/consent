using System.Linq;

namespace CHC.Consent.Common.Infrastructure
{
    public static class StringExtensions
    {
        public static string ToLowerCamel(this string input)
        {
             return input.First().ToString().ToLowerInvariant() + input.Substring(1);
        }

        public static string TrimEnd(this string input, string token)
        {
            return !input.EndsWith(token) 
                ? input : 
                input.Substring(0, input.Length - token.Length);
        }
    }
}