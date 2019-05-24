using System;
using System.Collections;
using System.Text;
using FluentAssertions.Formatting;

namespace CHC.Consent.EFCore.Tests
{
    public class DictionaryIdentifierFormatter : IValueFormatter
    {
        public bool CanHandle(object value) => value is IDictionary;

        /// <inheritdoc />
        public string Format(object value, FormattingContext context, FormatChild formatChild)
        {
            var newline = context.UseLineBreaks ? Environment.NewLine : "";
            var padding = new string('\t', context.Depth);

            var result = new StringBuilder($"{newline}{padding}{{");
            foreach (DictionaryEntry entry in (IDictionary)value)
            {
                result.AppendFormat(
                    "[{0}]: {{{1}}},",
                    formatChild("Key", entry.Key),
                    formatChild("Value", entry.Value));
            }

            result.Append($"{newline}{padding}}}");

            return result.ToString();
        }
        
        public static IValueFormatter Instance { get; } = new DictionaryIdentifierFormatter(); 
    }
}