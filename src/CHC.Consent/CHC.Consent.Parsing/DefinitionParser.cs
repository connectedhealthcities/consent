using CHC.Consent.Common.Identity.Identifiers;
using Sprache;
using System.Collections.Generic;
using System.Linq;

namespace CHC.Consent.Parsing
{
    /// <summary>
    /// <para>Parses definitions from a string of form: </para>
    /// <code> name : type </code>
    /// <code> name : enum ( 'value', 'value2', ... ) </code>
    /// <code> name : composite ( field: type, ... ) </code>
    /// </summary>
    /// <remarks>
    /// <para><c>name</c> should be a character (a-z, 0-9), followed by hyphen or character, ending with a character)</para>
    /// <para><c>type</c> is currently: string, date, integer, enum, composite</para>
    /// <para><c>enum</c> is <c>enum('value',...)</c> - values are wrapped in single quotes</para>
    /// <para><c>composite</c> is <c>composite( name:type, ... )</c> - these can be recursive</para> 
    /// </remarks>
    public class DefinitionParser
    {
        
        public IdentifierDefinition ParseString(string definition)
        {
            return Definition.Parse(definition);
        }

        private static readonly Parser<char> AToZ = Parse.Chars("abcdefghijklmnopqrstuvwxyz");
        private static readonly Parser<char> Digit = Parse.Chars("0123456789");

        private static readonly Parser<char> LetterOrDigit = AToZ.Or(Digit);

        private static readonly Parser<char> OpenBracket = Parse.Char('(').Named("an Open Bracket '('");
        private static readonly Parser<char> CloseBracket = Parse.Char(')').Named("a Close Bracket ')'");

        private static readonly Parser<char> Comma = Parse.Char(',').Named("a Comma ',' ");

        private static readonly Parser<string> SimpleIdentifier = LetterOrDigit.AtLeastOnce().Text();
        private static readonly Parser<string> HyphenPrefixedIdentifierPart = 
            from hyphen in Parse.Char('-').AtLeastOnce().Text()
            from part in LetterOrDigit.AtLeastOnce().Text()
            select string.Concat(hyphen, part);

        private static readonly Parser<string> Identifier =
            SimpleIdentifier.Then(
                start =>
                    HyphenPrefixedIdentifierPart.Many()
                    .Select(rest => string.Concat(start, string.Join("", rest))));

        private static readonly Parser<IIdentifierType> SimpleTypes = 
            Parse.String("string").Return((IIdentifierType)new StringIdentifierType())
                .XOr(Parse.String("date").Return((IIdentifierType)new DateIdentifierType()))
                .XOr(Parse.String("integer").Return((IIdentifierType)new IntegerIdentifierType()));

        private static readonly Parser<char> Quote = Parse.Char('\'').Named("a Single Quote");

        private static readonly Parser<string> QuotedString = 
            Parse.AnyChar.Except(Quote).XAtLeastOnce().Text().Contained(Quote, Quote);


        private static readonly Parser<IIdentifierType> EnumType = 
            from _ in Parse.String("enum").Text()
            from values in QuotedString.DelimitedBy(Comma).Contained(OpenBracket, CloseBracket)
            select new EnumIdentifierType(values.ToArray());

        private static Parser<IIdentifierType> Composite =>
            from _ in Parse.String("composite").Text()
            from parts in Parse.Ref(
                () => DefinitionList.Contained(OpenBracket, CloseBracket)
            ).Named("a Definition List").Token()
            select new CompositeIdentifierType(parts.Cast<IDefinition>().ToArray());

        private static Parser<IdentifierDefinition> Definition => 
            from name in Identifier.Named("a Name").Token()
            from _ in Parse.Char(':').Named("a Colon ':'")
            from type in SimpleTypes.XOr(EnumType).XOr(Composite).Named("a Type (string, date, etc...)").Token()
            select new IdentifierDefinition(name, type);

        private static Parser<IEnumerable<IdentifierDefinition>> DefinitionList =>
            Definition.Token().XDelimitedBy(Comma);
    }
}