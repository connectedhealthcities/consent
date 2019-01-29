using System;
using CHC.Consent.Common.Identity.Identifiers;
using Sprache;
using System.Linq;

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

        public static readonly Parser<IIdentifierType> SimpleTypes = 
            Parse.String("string").Return((IIdentifierType)new StringIdentifierType())
                .XOr(Parse.String("date").Return((IIdentifierType)new DateIdentifierType()))
                .XOr(Parse.String("integer").Return((IIdentifierType)new IntegerIdentifierType()));

        private static readonly Parser<char> Quote = Parse.Char('\'').Named("a Single Quote");

        private static readonly Parser<string> QuotedString = 
            Parse.AnyChar.Except(Quote).XAtLeastOnce().Text().Contained(Quote, Quote);

        public static readonly Parser<IIdentifierType> EnumType = 
            from _ in Parse.String("enum").Token().Text()
            from values in QuotedString.Token().DelimitedBy(Comma).Contained(OpenBracket, CloseBracket)
            select new EnumIdentifierType(values.ToArray());
    }
    
    
}