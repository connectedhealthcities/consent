using CHC.Consent.Common.Identity.Identifiers;
using FluentAssertions.Formatting;

namespace CHC.Consent.EFCore.Tests
{
    public class PersonIdentifierFormatter : IValueFormatter
    {
        /// <inheritdoc />
        public bool CanHandle(object value) => value is PersonIdentifier;

        /// <inheritdoc />
        public string Format(object value, FormattingContext context, FormatChild formatChild)
        {            
            var personIdentifier = (PersonIdentifier)value;
            return $"{personIdentifier.Definition.SystemName}: {formatChild("Value", personIdentifier.Value.Value)}";
        }
        
        public static IValueFormatter Instance { get; } = new PersonIdentifierFormatter();
    }
}