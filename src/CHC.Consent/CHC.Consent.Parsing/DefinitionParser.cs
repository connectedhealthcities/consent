using System;
using Sprache;
using System.Linq;
using CHC.Consent.Common.Infrastructure.Definitions;
using CHC.Consent.Common.Infrastructure.Definitions.Types;

namespace CHC.Consent.Parsing
{
    public static class DefinitionParser
    {
        private static readonly Parser<char> AToZ = Parse.Chars("abcdefghijklmnopqrstuvwxyz");
        private static readonly Parser<char> Digit = Parse.Chars("0123456789");
        private static readonly Parser<char> LetterOrDigit = AToZ.Or(Digit);
        public static readonly Parser<char> OpenBracket = Parse.Char('(').Named("an Open Bracket '('");
        public static readonly Parser<char> CloseBracket = Parse.Char(')').Named("a Close Bracket ')'");
        public static readonly Parser<char> Comma = Parse.Char(',').Named("a Comma ',' ");
        private static readonly Parser<string> SimpleIdentifier = LetterOrDigit.AtLeastOnce().Text();

        private static readonly Parser<string> HyphenPrefixedIdentifierPart = 
            from hyphen in Parse.Char('-').AtLeastOnce().Text()
            from part in LetterOrDigit.AtLeastOnce().Text()
            select String.Concat(hyphen, part);

        public static readonly Parser<string> Identifier = SimpleIdentifier.Then(
                start => HyphenPrefixedIdentifierPart.Many()
                        .Select(rest => String.Concat(start, String.Join("", rest))));

        public static readonly Parser<IDefinitionType> SimpleTypes = 
            Parse.String("string").Return(new StringDefinitionType())
                .XOr(Parse.String("date").Return((IDefinitionType)new DateDefinitionType()))
                .XOr(Parse.String("integer").Return((IDefinitionType)new IntegerDefinitionType()));

        private static readonly Parser<char> Quote = Parse.Char('\'').Named("a Single Quote");

        private static readonly Parser<string> QuotedString = 
            Parse.AnyChar.Except(Quote).XAtLeastOnce().Text().Contained(Quote, Quote);

        public static readonly Parser<IDefinitionType> EnumType = 
            from _ in Parse.String("enum").Token().Text()
            from values in QuotedString.Token().DelimitedBy(Comma).Contained(OpenBracket, CloseBracket)
            select new EnumDefinitionType(values.ToArray());
    }
    
    
}