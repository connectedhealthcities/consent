using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Parsing;
using CHC.Consent.Testing.Utils;
using FluentAssertions;
using Xunit;

namespace CHC.Consent.Tests.Identity
{
    public class DefinitionParserTests
    {
        public static TheoryData<string, IdentifierDefinition> Definitions =>
            new TheoryData<string, IdentifierDefinition>
            {
                {"a-string:string", Identifiers.Definitions.String("a-string")},
                {"a-date:date", Identifiers.Definitions.Date("a-date")},
                {"an-integer:integer", Identifiers.Definitions.Integer("an-integer")},
                {
                    "enum:enum('one','two','three')", 
                    Identifiers.Definitions.Enum(name: "enum","one", "two", "three")
                },
                {
                    "c:composite(one:string,two:date)",
                    Identifiers.Definitions.Composite(
                        "c",
                        Identifiers.Definitions.String("one"),
                        Identifiers.Definitions.Date("two"))
                }
            };
                    

        [Theory]
        [MemberData(nameof(Definitions))]
        public void CorrectlyParsesIdentifiers(string definitionString, IdentifierDefinition expected)
        {
            var definition = new DefinitionParser().ParseString(definitionString);

            definition.Should().BeEquivalentTo(expected);
        }
        
    }
}