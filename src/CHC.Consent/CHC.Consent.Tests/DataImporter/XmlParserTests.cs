using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CHC.Consent.Api.Client.Models;
using CHC.Consent.Common.Consent.Evidences;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.DataImporter.Features.ImportData;
using CHC.Consent.Testing.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using InternalIdentifierDefinition = CHC.Consent.Common.Identity.Identifiers.IdentifierDefinition;
using ClientIdentifierDefinition = CHC.Consent.Api.Client.Models.IdentifierDefinition;
using InternalEvidenceDefinition = CHC.Consent.Common.Consent.Evidences.EvidenceDefinition;
using ClientEvidenceDefinition = CHC.Consent.Api.Client.Models.EvidenceDefinition;

using IInternalDefinition = CHC.Consent.Common.Infrastructure.Definitions.IDefinition;
using IClientDefinition = CHC.Consent.Api.Client.Models.IDefinition;

namespace CHC.Consent.Tests.DataImporter
{
    public class XmlParserTests
    {
        private const string XmlImportFileBaseUri = "test.xml";

        /// <inheritdoc />
        public XmlParserTests(ITestOutputHelper output)
        {
            Logger = new XunitLogger<XmlParser>(output, "Parse"); 
        }

        private static readonly Dictionary<string, Type> EmptyTypeMap = new Dictionary<string, Type>();
        private XunitLogger<XmlParser> Logger { get; }

        private static IIdentifierValueDto ParseIdentifierString(string fullXml, params InternalIdentifierDefinition[] definitions)
        {
            var parser = CreateXmlParser(definitions);
            var xDocument = CreateXDocumentWithLineInfo(fullXml);
            var identifier = parser.ParseIdentifier(xDocument.Root);

            Assert.NotNull(identifier);
            return identifier;
        }

        private static ImportedPersonSpecification[] ParsePeople(string fullXml, params InternalIdentifierDefinition[] definitions)
        {
            var parse = CreateXmlParser(definitions);
            return parse.GetPeople(CreateXmlReader(fullXml)).ToArray();
        }

        private static XmlParser CreateXmlParser(IEnumerable<InternalIdentifierDefinition> definitions)
        {
            return new XmlParser(definitions.ConvertToClientDefinitions(), Array.Empty<ClientEvidenceDefinition>());
        }

        private ImportedConsentSpecification ParseConsent(
            string xml,
            IdentifierDefinitionRegistry personIdentifierTypes = null,
            EvidenceDefinitionRegistry evidenceDefinitions=null)
        {
            return new XmlParser(
                    personIdentifierTypes?.ConvertToClientDefinitions() ?? Array.Empty<ClientIdentifierDefinition>(),
                    evidenceDefinitions?.ConvertToClientDefinitions() ?? Array.Empty<ClientEvidenceDefinition>()
                )
                .ParseConsent(CreateXDocumentWithLineInfo(xml).Root);
        }

        private static XDocument CreateXDocumentWithLineInfo(string fullXml) =>
            XDocument.Load(
                CreateXmlReader(fullXml),
                LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);

        private static XmlReader CreateXmlReader(string fullXml)
        {
            return XmlReader.Create(new StringReader(fullXml), new XmlReaderSettings{IgnoreWhitespace = true, IgnoreComments = true},
                new XmlParserContext(null, null, null, null, null, null, baseURI:XmlImportFileBaseUri, null, XmlSpace.Default));
        }

        private static XElement IdentifierElement(string type, params object[] value)
        {
            return new XElement("identifier", new XAttribute("type", type), value);
        }

        private static IdentifierValueParser CreateParser(
            params InternalIdentifierDefinition[] definitions)
            => IdentifierValueParser.CreateFrom(definitions.ConvertToClientDefinitions());

        [Fact]
        public void CanParseSexIdentifier()
        {
            var sexIdentifier = ParseIdentifierString(
                @"<identifier type=""sex"">Male</identifier>",
                Identifiers.Definitions.Sex
                );

            sexIdentifier.Should().BeOfType<IdentifierValueDtoString>()
                .Which.Value.Should().Be("Male");
        }

        [Fact]
        public void CanParseDateOfBirthIdentifier()
        {
            var parser = CreateParser(Identifiers.Definitions.DateOfBirth);

            var identifierValue = parser.Parse(IdentifierElement("date-of-birth", "2016-03-14"));

            identifierValue.Should().BeEquivalentTo(Identifiers.Definitions.DateOfBirth.Value(14.March(2016)));
            
        }

        [Fact]
        public void CanParseNameIdentifier()
        {
            var parser = CreateParser(Identifiers.Definitions.Name);

            var identifierValue = parser.Parse(
                IdentifierElement("name",
                    IdentifierElement("given", "Fred"),
                    IdentifierElement("family", "Whitton")
                    )
            );
            
            identifierValue.Should().BeEquivalentTo(ClientIdentifierValues.Name("Fred", "Whitton"));
        }

        [Fact]
        public void CanParsePartialAddress()
        {
            var parser = CreateParser(Identifiers.Definitions.Address);

            var identifierValue = parser.Parse(
                IdentifierElement(
                    "address",
                    IdentifierElement("line-1", "1 Testing Street"),
                    IdentifierElement("postcode", "TS1 3ST"))
            );
            
            identifierValue.Should().BeEquivalentTo(ClientIdentifierValues.Address("1 Testing Street",  postcode: "TS1 3ST"));
        }

        [Fact]
        public void ReportsErrorOnIncorrectType()
        {
            var parseException = Assert.ThrowsAny<XmlParseException>(() => ParseIdentifierString(@"<identifier type=""unknown"" />"));

            Assert.Contains("identifier", parseException.Message);
            Assert.True(parseException.HasLineInfo);
            Assert.Equal(1, parseException.LineNumber);
            Assert.Equal(2, parseException.LinePosition);
            
        }

        [Fact]
        public void PeopleCollectionShouldHaveAuthorityAttribute()
        {
            Assert.ThrowsAny<Exception>(
                    () => ParsePeople("<people></people>")
            ).Message.Should().Contain("authority");

            ParsePeople("<people authority=\"none\"></people>");
        }

        [Fact]
        public void CanParseWholePerson()
        {
            const string personXml = @"<?xml version=""1.0""?>
<people authority=""test"">
    <person>
        <identity>
            <identifier type=""nhs-number"">4099999999</identifier>
            <identifier type=""bradford-hospital-number"">RAE9999999</identifier>
            <identifier type=""name""><identifier type=""given"">Jo</identifier><identifier type=""family"">Bloggs</identifier></identifier>
            <identifier type=""date-of-birth"">1990-06-16</identifier>
            <identifier type=""address"">
                <identifier type=""line-1"">22 Love Street</identifier>
                <identifier type=""line-2"">Holmestown</identifier>
                <identifier type=""line-3"">Bradtopia</identifier>
                <identifier type=""line-4"">West Yorkshire</identifier>
                <identifier type=""postcode"">BD92 4FX</identifier>
            </identifier>
        </identity>
        <lookup>
            <match><identifier type=""nhs-number"">4099999999</identifier></match>
            <match><identifier type=""bradford-hospital-number"">RAE9999999</identifier></match>
        </lookup>
        <consent date-given=""2017-10-14"" study-id=""1"">
			<given-by>
				<match><identifier type=""nhs-number"">4099999999</identifier></match>
				<match><identifier type=""bradford-hospital-number"">RAE9999999</identifier></match>
			</given-by>
            <evidence>
                <evidence type=""medway"">
                    <evidence type=""consent-taken-by"">Betsey Trotwood</evidence>
                </evidence>
            </evidence>
        </consent>
    </person>
</people>";


            var xmlReader = CreateXmlReader(personXml);
            var specification = new XmlParser(
                    Identifiers.Registry.ConvertToClientDefinitions(),
                    KnownEvidence.Registry.ConvertToClientDefinitions())
                .GetPeople(xmlReader)
                .Single();
            var person = specification.PersonSpecification;


            Assert.DoesNotContain(person.Identifiers, identifier => identifier == null);
            Assert.Collection(
                person.Identifiers,
                Matches.NhsNumber("4099999999"),
                Matches.HospitalNumber("RAE9999999"),
                Matches.Name("Jo", "Bloggs"),
                Matches.DateOfBirth(16.June(1990)),
                Matches.Address("22 Love Street", "Holmestown", "Bradtopia", "West Yorkshire", postcode: "BD92 4FX")
            );

            person.MatchSpecifications
                .Should()
                .BeEquivalentTo(
                    new IdentifierMatchSpecification(Identifiers.Definitions.NhsNumber.Value("4099999999")),
                    new IdentifierMatchSpecification(Identifiers.Definitions.HospitalNumber.Value("RAE9999999"))
                );
        }


        [Fact]
        public void CanParseSimpleConsent()
        {
            var consentSpec = ParseConsent(@"<consent date-given=""2017-03-12"" study-id=""42"" />");
                
            Assert.NotNull(consentSpec);
            Assert.Equal(12.March(2017), consentSpec.DateGiven);
            Assert.Equal(42L, consentSpec.StudyId);
            Assert.Empty(consentSpec.GivenBy);
            Assert.Empty(consentSpec.Evidence);
        }

        [Fact]
        public async void ReportsErrorWithUnknownMatchIdentifier()
        {
            var exception = await Assert.ThrowsAsync<XmlParseException>(
                () =>
                    Task.FromResult(
                        ParseConsent(
                            @"<consent date-given=""2017-03-12"" study-id=""42"" xmlns:err=""error"" >
                        <givenBy><match><identifier type=""error"">45</identifier></match></givenBy>
                    </consent>"
                        )));
            
            Assert.Contains("'error'", exception.Message);
            Assert.NotEqual(0, exception.LineNumber);
        }

        [Fact]
        public void ReportsErrorWithUnknownEvidence()
        {
            var exception = Assert.Throws<XmlParseException>(
                () => ParseConsent(
                    
                        @"<consent date-given=""2017-03-12"" study-id=""42"" xmlns:err=""error"" >
    <evidence>
        <evidence type=""error"">this is an error</evidence>
    </evidence>
</consent>",
                    evidenceDefinitions: KnownEvidence.Registry 
                    ));
            
            Assert.Contains("'error'", exception.Message);
            Assert.NotEqual(0, exception.LineNumber);
        }

        [Fact]
        public void CorrectlyParsedConsentedMatchSpecification()
        {
            ParseMatchSpecification("<consented study-id=\"123\" />")
                .Should()
                .BeEquivalentTo(new ConsentedForStudyMatchSpecification(123));
        }

        [Fact]
        public void ThrowsExceptionForConsentedMatchSpecificationWithoutStudyId()
        {
            Action parse = () => ParseMatchSpecification("<consented />");
            parse.Should()
                .ThrowExactly<XmlParseException>()
                .WithMessage("*study-id*");
        }

        [Fact]
        public void ThrowsExceptionForContentedMatchSpecificationWithNonNumericStudyId()
        {
            Action parse = () => ParseMatchSpecification("<consented study-id=\"abc\" />");

            parse.Should().ThrowExactly<XmlParseException>()
                .WithMessage("*abc*");
        }

        [Fact]
        public void CorrectlyParsesPersonAgencyIdMatchSpecification()
        {
            ParseMatchSpecification("<agency name=\"test\" id=\"123-456\" />")
                .Should()
                .BeEquivalentTo(new PersonAgencyIdMatchSpecification("test","123-456"));
        }

        [Fact]
        public void ThrowsExceptionForPersonAgencyIdMatchSpecificationWithoutNameAttribute()
        {
            Action parse = () => ParseMatchSpecification("<agency id=\"123-214\" />");

            parse.Should().ThrowExactly<XmlParseException>()
                .WithMessage("*agency*");
        }
        
        [Fact]
        public void ThrowsExceptionForPersonAgencyIdMatchSpecificationWithoutIdAttribute()
        {
            Action parse = () => ParseMatchSpecification("<agency name=\"test\" />");

            parse.Should().ThrowExactly<XmlParseException>()
                .WithMessage("*id*");
        }

        [Fact]
        public void CorrectlyParsesCompositeMatchSpecification()
        {
            CreateXmlParser(Identifiers.Registry)
                .ParseMatchSpecification(
            new XElement("match", 
                new XElement("identifier", new XAttribute("type", "nhs-number"), "123"),
                new XElement("consented", new XAttribute("study-id", 456L))
            )
                    ).Should().BeEquivalentTo(
                    new CompositeMatchSpecification(
                        new IdentifierMatchSpecification(Identifiers.Definitions.NhsNumber.Value("123")),
                        new ConsentedForStudyMatchSpecification(456)
                    )
                );

        }
        
        private static MatchSpecification ParseMatchSpecification(string xml)
        {
            return CreateXmlParser(Identifiers.Registry)
                .ParseMatchSpecification(CreateXDocumentWithLineInfo(xml));
        }

        [Fact]
        public void CorrectlyParsesFullConsent()
        {
            var consent = ParseConsent(
                @"<consent date-given=""2017-03-12"" study-id=""42"">
                    <givenBy>
                        <match><identifier type=""nhs-number"">8877881</identifier></match>
                        <match><identifier type=""bradford-hospital-number"">1122112</identifier></match>
                    </givenBy>
                    <evidence>
                        <evidence type=""medway"">
                            <evidence type=""competent-status"">Delegated</evidence>
                            <evidence type=""consent-given-by"">Mother</evidence>
                            <evidence type=""consent-taken-by"">Betsey Trotwood</evidence>
                        </evidence>
                    </evidence>
                </consent>",
                personIdentifierTypes: Identifiers.Registry,
                KnownEvidence.Registry
            );

            consent.GivenBy
                .Should()
                .BeEquivalentTo(
                    new IdentifierMatchSpecification(Identifiers.Definitions.NhsNumber.Value("8877881")),
                    new IdentifierMatchSpecification(Identifiers.Definitions.HospitalNumber.Value("1122112"))
                );
            
            
            Assert.Collection(
                consent.Evidence,
                e =>
                {
                    e.Should().BeEquivalentTo(
                        Evidences.ClientMedwayDto("Delegated", "Mother", "Betsey Trotwood"),
                        o => o.RespectingRuntimeTypes()
                        );
                },
                e =>
                {
                    e.Should().BeEquivalentTo(
                        KnownEvidence.ImportFile.ClientDto(
                            KnownEvidence.ImportFileParts.BaseUri.ClientDto(XmlImportFileBaseUri),
                            KnownEvidence.ImportFileParts.LineNumber.ClientDto(1),
                            KnownEvidence.ImportFileParts.LinePosition.ClientDto(2)
                            ),
                        o => o.RespectingRuntimeTypes()
                    );
                }
            );

        }
    }
}