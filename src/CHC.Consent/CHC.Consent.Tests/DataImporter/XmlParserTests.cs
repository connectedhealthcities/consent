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
using CHC.Consent.DataImporter;
using CHC.Consent.Testing.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using InternalIdentifierDefinition = CHC.Consent.Common.Identity.Identifiers.IdentifierDefinition;
using InternalIdentifierType = CHC.Consent.Common.Identity.Identifiers.IIdentifierType;
using ClientIdentifierDefinition = CHC.Consent.Api.Client.Models.IdentifierDefinition;
using ClientIdentifierType = CHC.Consent.Api.Client.Models.IIdentifierType;

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

        private IIdentifierValueDto ParseIdentifierString(string fullXml, params InternalIdentifierDefinition[] definitions)
        {
            var xDocument = CreateXDocumentWithLineInfo(fullXml);
            
            var identifier = new XmlParser(Logger, ConvertToClientType(definitions)).ParseIdentifier(xDocument.Root);

            Assert.NotNull(identifier);
            return identifier;
        }

        private List<ClientIdentifierDefinition> ConvertToClientType(IEnumerable<InternalIdentifierDefinition> definitions)
        {
            return definitions.Select(ConvertToClientType).ToList();
        }

        private ClientIdentifierDefinition ConvertToClientType(InternalIdentifierDefinition definition)
        {
            return new ClientIdentifierDefinition(
                definition.Name,
                definition.SystemName,
                ConvertToClientType(definition.Type));
        }

        private ClientIdentifierType ConvertToClientType(InternalIdentifierType internalType)
        {
            switch (internalType)
            {
                case CHC.Consent.Common.Identity.Identifiers.CompositeIdentifierType composite:
                    return new CHC.Consent.Api.Client.Models.CompositeIdentifierType(composite.SystemName,
                        composite.Identifiers.Select(ConvertToClientType).ToList());
                case CHC.Consent.Common.Identity.Identifiers.DateIdentifierType date:
                    return new CHC.Consent.Api.Client.Models.DateIdentifierType(date.SystemName);
                case CHC.Consent.Common.Identity.Identifiers.EnumIdentifierType @enum:
                    return new CHC.Consent.Api.Client.Models.EnumIdentifierType(systemName:@enum.SystemName, values:@enum.Values.ToList());
                case CHC.Consent.Common.Identity.Identifiers.IntegerIdentifierType integer:
                    return new CHC.Consent.Api.Client.Models.IntegerIdentifierType(integer.SystemName);
                case CHC.Consent.Common.Identity.Identifiers.StringIdentifierType @string:
                    return new CHC.Consent.Api.Client.Models.StringIdentifierType(@string.SystemName);
                default:
                    throw new ArgumentOutOfRangeException(nameof(internalType));
            }
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

        [Fact]
        public void CanParseSexIdentifier()
        {
            var xml = @"<identifier type=""sex"">Male</identifier>";
            
            var sexIdentifier = ParseIdentifierString(
                xml,
                Identifiers.Definitions.Sex
                );

            sexIdentifier.Should().BeOfType<IdentifierValueDtoString>()
                .Subject.Value
                .Should().Be("Male");
        }


        private static XElement IdentifierElement(string type, params object[] value)
        {
            return new XElement("identifier", new XAttribute("type", type), value);
        }

        [Fact]
        public void CanParseDateOfBirthIdentifier()
        {
            var parser = new IdentifierValueParser(
                new [] {ConvertToClientType(Identifiers.Definitions.DateOfBirth)});

            var identifierValue = parser.Parse(IdentifierElement("date-of-birth", "2016-03-14"));

            identifierValue.Should().BeEquivalentTo(Identifiers.Definitions.DateOfBirth.Value(14.March(2016)));
            
        }
        
        [Fact]
        public void CanParseNameIdentifier()
        {
            var parser = new IdentifierValueParser(((IDictionary<string, ClientIdentifierDefinition>) new Dictionary<string, ClientIdentifierDefinition>
            {
                ["name"] =
                    ConvertToClientType(Identifiers.Definitions.Name)
            }).Values.ToList());

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
            var parser = new IdentifierValueParser(((IDictionary<string, ClientIdentifierDefinition>) new Dictionary<string, ClientIdentifierDefinition>
            {
                ["address"] =
                    ConvertToClientType(Identifiers.Definitions.Address)
            }).Values.ToList());

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
        public void CanParseWholePerson()
        {
            const string personXml = @"<?xml version=""1.0""?>
<people xmlns:b4acase=""uk.nhs.bradfordhospitals.bib4all.consent"" xmlns:b4aevidence=""uk.nhs.bradfordhospitals.bib4all.evidence"">
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
            <case>
                <b4acase:pregnancyNumber>2</b4acase:pregnancyNumber>
            </case>
            <evidence>
                <b4aevidence:medway>
                    <competentStatus></competentStatus>
                    <consentGivenBy></consentGivenBy>
                    <consentTakenBy>Betsey Trotwood</consentTakenBy>
                </b4aevidence:medway>
            </evidence>
        </consent>
    </person>
</people>";

            
            var xmlReader = CreateXmlReader(personXml);
            var specification = new XmlParser(Logger, ConvertToClientType(Identifiers.Registry.Values)).GetPeople(xmlReader).Single();
            var person = specification.PersonSpecification;

            Action<IIdentifierValueDto> Address(
                string line1 = null,
                string line2 = null,
                string line3 = null,
                string line4 = null,
                string line5 = null,
                string postcode = null) =>
                _ => AssertAddress(_, line1, line2, line3, line4, line5, postcode);
            

            Assert.DoesNotContain(person.Identifiers, identifier => identifier == null);
            Assert.Collection(person.Identifiers,
                NhsNumber("4099999999"),
                HospitalNumber("RAE9999999"),
                Name("Jo", "Bloggs"),
                DateOfBirth(16.June(1990)),
                Address("22 Love Street", "Holmestown", "Bradtopia", "West Yorkshire", postcode: "BD92 4FX")
                );
            
            Assert.Collection(person.MatchSpecifications,
                m => Assert.Collection(m.Identifiers, NhsNumber("4099999999")),
                m => Assert.Collection(m.Identifiers, HospitalNumber("RAE9999999"))
            );
        }

        private ImportedConsentSpecification ParseConsent(
            string xml,
            IEnumerable<InternalIdentifierDefinition> personIdentifierTypes = null,
            Dictionary<string, Type> evidenceTypes = null)
        {

            return new XmlParser(
                    Logger,
                    ConvertToClientType(personIdentifierTypes ?? Enumerable.Empty<InternalIdentifierDefinition>()))
                .ParseConsent(
                    CreateXDocumentWithLineInfo(xml).Root,
                    evidenceTypes ?? EmptyTypeMap);
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
        <err:error>45</err:error>
    </evidence>
</consent>",
                    evidenceTypes: new Dictionary<string, Type>
                    {
                        ["test"]= typeof(UkNhsBradfordhospitalsBib4allEvidenceMedway) 
                    }
                    ));
            
            Assert.Contains("error.error", exception.Message);
            Assert.NotEqual(0, exception.LineNumber);
        }

        [Fact]
        public void CorrectlyParsesFullConsent()
        {


            var consent = ParseConsent(
                @"<consent date-given=""2017-03-12"" study-id=""42"" xmlns:nhs=""uk.nhs"" xmlns:bfd=""uk.nhs.bradfordhospitals"" xmlns:b4acase=""uk.nhs.bradfordhospitals.bib4all.consent"" xmlns:b4aevidence=""uk.nhs.bradfordhospitals.bib4all.evidence"">
                    <givenBy>
                        <match><identifier type=""nhs-number"">8877881</identifier></match>
                        <match><identifier type=""bradford-hospital-number"">1122112</identifier></match>
                    </givenBy>
                    <evidence>
                        <b4aevidence:medway>
                            <competentStatus>Delegated</competentStatus>
                            <consentGivenBy>Mother</consentGivenBy>
                            <consentTakenBy>Betsey Trotwood</consentTakenBy>
                        </b4aevidence:medway>
                    </evidence>
                </consent>",
                personIdentifierTypes: Identifiers.Registry.Values,
                
                evidenceTypes: new Dictionary<string, Type>
                {
                    [MedwayEvidence.TypeName] = typeof(UkNhsBradfordhospitalsBib4allEvidenceMedway)
                }
            );

            Assert.Collection(
                consent.GivenBy,
                m => Assert.Collection(m.Identifiers, NhsNumber("8877881")),
                m => Assert.Collection(m.Identifiers, HospitalNumber("1122112"))
            );
            
            Assert.Collection(
                consent.Evidence,
                e =>
                {
                    var medway = Assert.IsType<UkNhsBradfordhospitalsBib4allEvidenceMedway>(e);
                    Assert.Equal("Delegated", medway.CompetentStatus);
                    Assert.Equal("Mother", medway.ConsentGivenBy);
                    Assert.Equal("Betsey Trotwood", medway.ConsentTakenBy);
                },
                e =>
                {
                    var fileInfo = Assert.IsType<OrgConnectedhealthcitiesImportFileSource>(e);
                    Assert.Equal(XmlImportFileBaseUri, fileInfo.BaseUri);
                    Assert.Equal(1, fileInfo.LineNumber);
                    Assert.Equal(2, fileInfo.LinePosition);
                }
            );

        }

        private static Action<IIdentifierValueDto> NhsNumber(string expectedValue) =>
            v => v.Should().BeEquivalentTo(Identifiers.Definitions.NhsNumber.Value(expectedValue));

        private static Action<IIdentifierValueDto> HospitalNumber(string expectedValue) =>
            v => v.Should().BeEquivalentTo(Identifiers.Definitions.HospitalNumber.Value( expectedValue));
            
        
        
        


        private static Action<IIdentifierValueDto> DateOfBirth(DateTime dateofBirth) =>
            v => v.Should().BeEquivalentTo(Identifiers.Definitions.DateOfBirth.Value( dateofBirth.Date));

        private static Action<IIdentifierValueDto> Name(string firstName, string lastName) => 
            v => v.Should().BeEquivalentTo(Identifiers.Definitions.Name.Value( new []
            {
                Identifiers.Definitions.FirstName.Value(firstName),
                Identifiers.Definitions.LastName.Value(lastName)
            }));


        private static void AssertAddress(
            IIdentifierValueDto address,
            string line1 = null,
            string line2 = null,
            string line3 = null,
            string line4 = null,
            string line5 = null,
            string postcode = null)
        {
            address.Should().BeEquivalentTo(
                ClientIdentifierValues.Address(
                    line1,
                    line2,
                    line3,
                    line4,
                    line5,
                    postcode)
            );
        }
    }
}