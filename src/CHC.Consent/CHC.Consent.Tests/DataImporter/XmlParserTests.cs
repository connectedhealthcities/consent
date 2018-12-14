using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CHC.Consent.Api.Client.Models;
using CHC.Consent.Common.Consent.Evidences;
using CHC.Consent.Common.Consent.Identifiers;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.DataImporter;
using CHC.Consent.Testing.Utils;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog.Core;
using Xunit;
using Xunit.Abstractions;

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

        private T ParseIdentifier<T>(string innerXml)
        {
            return ParseIdentifier<T>(innerXml, new Dictionary<string, Type> {["identifier"] = typeof(T)});
        }

        private T ParseIdentifier<T>(string innerXml, Dictionary<string, Type> typeMap)
        {
            //This dance ensures we have line numbers (not that they mean a lot here)
            var fullXml = $"<identifier>{innerXml}</identifier>";
            return ParseIdentifierString<T>(fullXml, typeMap);
        }

        private T ParseIdentifierString<T>(string fullXml, Dictionary<string, Type> typeMap)
        {
            var xDocument = CreateXDocumentWithLineInfo(fullXml);

            var identifier = new XmlParser(Logger).ParseIdentifier(xDocument.Root, typeMap);

            Assert.NotNull(identifier);
            return Assert.IsType<T>(identifier);
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
            var xml = @"<sex>Male</sex>";
            
            var sexIdentifier = ParseIdentifierString<Sex>(
                xml,
                new Dictionary<string, Type>
                {
                    ["sex"] = typeof(Sex) 
                }
                );
            
            Assert.Equal("Male", sexIdentifier.Value);
        }

        class DummyIdentifierWithOneArg<T> : IPersonIdentifier
        {
            public T Value { get; }

            /// <inheritdoc />
            public DummyIdentifierWithOneArg(T value)
            {
                Value = value;
            }
        }

        [Fact]
        public void Matches_CamelCasedElementName_WithKebabCasedTypeName()
        {
            const string xml = @"<tst:dummyIdentifier xmlns:tst=""test"">Male</tst:dummyIdentifier>";
            
            var identifier = ParseIdentifierString<DummyIdentifierWithOneArg<string>>(
                xml,
                new Dictionary<string, Type>
                {
                    ["test.dummy-identifier"] = typeof(DummyIdentifierWithOneArg<string>) 
                }
            );
            
            Assert.Equal("Male", identifier.Value);
        }
        
        [Fact]
        public void Matches_CamelCasedElementNameInGlobalNS_WithKebabCasedTypeName()
        {
            const string xml = @"<dummyIdentifier>Male</dummyIdentifier>";
            
            var identifier = ParseIdentifierString<DummyIdentifierWithOneArg<string>>(
                xml,
                new Dictionary<string, Type>
                {
                    ["dummy-identifier"] = typeof(DummyIdentifierWithOneArg<string>) 
                }
            );
            
            Assert.Equal("Male", identifier.Value);
        }

        [Fact]
        public void ParsesNullableEnumCorrectly()
        {
            var identifier = ParseIdentifierString<DummyIdentifierWithOneArg<DayOfWeek?>>(
                "<test />",
                new Dictionary<string, Type>
                {
                    ["test"] = typeof(DummyIdentifierWithOneArg<DayOfWeek?>)
                });
            
            Assert.Null(identifier.Value);
        }

        [Fact]
        public void CanParseDateOfBirthIdentifier()
        {
            AssertDateOfBirth(14.March(2016), ParseIdentifier<DateOfBirth>("2016-03-14"));
        }
        
        [Fact]
        public void CanParseDateOfBirthIdentifierAsValue()
        {
            AssertDateOfBirth(14.March(2016), ParseIdentifier<DateOfBirth>("<value>2016-03-14</value>"));
        }

        [Fact]
        public void CanParseNameIdentifier()
        {
            var nameIdentifier = ParseIdentifier<Name>("<first-name>Bart</first-name><last-name>Simpson</last-name>");
            AssertName("Bart", "Simpson", nameIdentifier);
        }

        [Fact]
        public void CanParsePartialAddress()
        {
            var address =
                ParseIdentifier<Address>(
                    "<line-1>line 1</line-1><postcode>T3 3ST</postcode>");
            
            AssertAddress(address, line1:"line 1", postcode:"T3 3ST");
        }

        [Fact]
        public void ReportsErrorOnIncorrectType()
        {
            var parseException = Assert.ThrowsAny<XmlParseException>(() => ParseIdentifier<NhsNumber>("", EmptyTypeMap));

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
            <nhs-number>4099999999</nhs-number>
            <bradford-hospital-number>RAE9999999</bradford-hospital-number>
            <name><first-name>Jo</first-name><last-name>Bloggs</last-name></name>
            <date-of-birth>1990-06-16</date-of-birth>
            <address>
                <line-1>22 Love Street</line-1>
                <line-2>Holmestown</line-2>
                <line-3>Bradtopia</line-3>
                <line-4>West Yorkshire</line-4>
                <postcode>BD92 4FX</postcode>
            </address>
        </identity>
        <lookup>
            <match><nhs-number>4099999999</nhs-number></match>
            <match><bradford-hospital-number>RAE9999999</bradford-hospital-number></match>
        </lookup>
        <consent date-given=""2017-10-14"" study-id=""1"">
			<given-by>
				<match><nhs-number>4099999999</nhs-number></match>
				<match><bradford-hospital-number>RAE9999999</bradford-hospital-number></match>
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
            var specification = new XmlParser(Logger).GetPeople(xmlReader).Single();
            var person = specification.PersonSpecification;

            Action<IPersonIdentifier> Address(
                string line1 = null,
                string line2 = null,
                string line3 = null,
                string line4 = null,
                string line5 = null,
                string postcode = null) =>
                AssertIdentifier<Address>(
                    _ => AssertAddress(_, line1, line2, line3, line4, line5, postcode));
            

//            Assert.DoesNotContain(person.Identifiers, identifier => identifier == null);
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
            Dictionary<string, Type> personIdentifierTypes = null,
            Dictionary<string, Type> consentTypes = null,
            Dictionary<string, Type> evidenceTypes = null)
        {
            return new XmlParser(Logger).ParseConsent(
                CreateXDocumentWithLineInfo(xml).Root,
                personIdentifierTypes ?? EmptyTypeMap,
                consentTypes ?? EmptyTypeMap,
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
            Assert.Empty(consentSpec.CaseId);
        }
        
        [Fact]
        public void ReportsErrorWithUnknownMatchIdentifier()
        {
            var exception = Assert.Throws<XmlParseException>(
                () => ParseConsent(
                    @"<consent date-given=""2017-03-12"" study-id=""42"" xmlns:err=""error"" >
                        <givenBy><match><err:error>45</err:error></match></givenBy>
                    </consent>",
                    personIdentifierTypes: new Dictionary<string, Type>
                    {
                        ["uk.test"] = typeof(NhsNumber)
                    }));
            
            Assert.Contains("error.error", exception.Message);
            Assert.NotEqual(0, exception.LineNumber);
        }
        
        [Fact]
        public void ReportsErrorWithUnknownCaseIdentifier()
        {
            var exception = Assert.Throws<XmlParseException>(
                () => ParseConsent(

                    @"<consent date-given=""2017-03-12"" study-id=""42"" xmlns:err=""error"" >
                        <case>
                            <err:error>45</err:error>
                        </case>
                    </consent>",
                    consentTypes: new Dictionary<string, Type>
                    {
                        ["bib4all.pregnancyId"] = typeof(UkNhsBradfordhospitalsBib4allConsentPregnancyNumber)
                    }
                ));
            
            Assert.Contains("error.error", exception.Message);
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
                        <match><nhs-number>8877881</nhs-number></match>
                        <match><bradford-hospital-number>1122112</bradford-hospital-number></match>
                    </givenBy>
                    <case>
                        <b4acase:pregnancyNumber>3</b4acase:pregnancyNumber>
                    </case>
                    <evidence>
                        <b4aevidence:medway>
                            <competentStatus>Delegated</competentStatus>
                            <consentGivenBy>Mother</consentGivenBy>
                            <consentTakenBy>Betsey Trotwood</consentTakenBy>
                        </b4aevidence:medway>
                    </evidence>
                </consent>",
                personIdentifierTypes: new Dictionary<string, Type>
                {
                    ["bradford-hospital-number"] = typeof(BradfordHospitalNumber),
                    ["nhs-number"] = typeof(NhsNumber),
                },
                consentTypes: new Dictionary<string, Type>
                {
                    [PregnancyNumberIdentifier.TypeName] = typeof(UkNhsBradfordhospitalsBib4allConsentPregnancyNumber)
                },
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
                consent.CaseId,
                id => Assert.Equal("3", Assert.IsType<UkNhsBradfordhospitalsBib4allConsentPregnancyNumber>(id).Value)
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
        
        

        private static Action<IPersonIdentifier> NhsNumber(string expectedValue) => AssertIdentifier<NhsNumber>(
            i => AssertNhsNumber(expectedValue, i));

        private static void AssertNhsNumber(string expectedValue, NhsNumber identifier) => Assert.Equal(expectedValue, identifier.Value);

        private static Action<IPersonIdentifier> HospitalNumber(string expectedValue) => AssertIdentifier<BradfordHospitalNumber>(
            i => AssertHospitalNumber(expectedValue, i));

        private static void AssertHospitalNumber(string expectedValue, BradfordHospitalNumber i) => Assert.Equal(expectedValue, i.Value);

        private static Action<IPersonIdentifier> DateOfBirth(DateTime dateofBirth) => AssertIdentifier<DateOfBirth>(i => AssertDateOfBirth(dateofBirth, i));

        private static void AssertDateOfBirth(DateTime dateofBirth, DateOfBirth i) => Assert.Equal(dateofBirth, i.Value);

        private static Action<IPersonIdentifier> Name(string firstName, string lastName) => AssertIdentifier<Name>(_ => { AssertName(firstName, lastName, _); });

        private static void AssertName(string firstName, string lastName, Name identifier)
        {
            Assert.Equal(firstName, identifier.Value?.FirstName);
            Assert.Equal(lastName, identifier.Value?.LastName);
        }

        private static Action<IPersonIdentifier> AssertIdentifier<T>(Action<T> test) => _ => test(Assert.IsType<T>(_));

        private static void AssertAddress(
            Address address,
            string line1 = null,
            string line2 = null,
            string line3 = null,
            string line4 = null,
            string line5 = null,
            string postcode = null)
        {
            Assert.Equal(line1, address.Value?.Line1);
            Assert.Equal(line2, address.Value?.Line2);
            Assert.Equal(line3, address.Value?.Line3);
            Assert.Equal(line4, address.Value?.Line4);
            Assert.Equal(line5, address.Value?.Line5);
            Assert.Equal(postcode, address.Value?.Postcode);
        }
    }
}