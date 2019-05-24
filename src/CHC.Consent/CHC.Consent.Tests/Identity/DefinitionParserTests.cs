using CHC.Consent.Common.Consent.Evidences;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure.Definitions.Types;
using CHC.Consent.Parsing;
using CHC.Consent.Testing.Utils;
using FluentAssertions;
using Xunit;

namespace CHC.Consent.Tests.Identity
{
    public class DefinitionParserTests
    {
        private readonly DefinitionParser<IdentifierDefinition> parser = 
            new IdentifierDefinitionParser();

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
            var definition = parser.ParseString(definitionString);

            definition.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void CanParseEvidence()
        {
            var evidenceParser = new DefinitionParser<EvidenceDefinition>(EvidenceDefinition.Create);

            var definition =  evidenceParser.ParseString(
                @"    
                medway: composite (
                    competent-status : enum ( 'Yes', 'No' ),
                    consent-given-by : string,
                    consent-taken-by : string
                )
            ");

            definition.Should().BeEquivalentTo(
                EvidenceDefinition.Create(
                    "medway",
                    new CompositeDefinitionType(
                        EvidenceDefinition.Create("competent-status", new EnumDefinitionType("Yes", "No")),
                        EvidenceDefinition.Create("consent-given-by", new StringDefinitionType()),
                        EvidenceDefinition.Create("consent-taken-by", new StringDefinitionType())
                    ))
            );
        }
        
    }
}