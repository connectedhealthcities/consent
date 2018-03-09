using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CHC.Consent.Api.Client.Models;
using CHC.Consent.DataImporter;
using CHC.Consent.Testing.Utils;
using Xunit;
using Sex = CHC.Consent.Common.Sex;

namespace CHC.Consent.Tests.DataImporter
{
    public class XmlParserTests
    {
        private static readonly Dictionary<string, Type> EmptyTypeMap = new Dictionary<string, Type>();

        private static T ParseIdentifier<T>(string innerXml)
        {
            return ParseIdentifier<T>(innerXml, new Dictionary<string, Type> {["identifier"] = typeof(T)});
        }

        private static T ParseIdentifier<T>(string innerXml, Dictionary<string, Type> typeMap)
        {
            //This dance ensures we have line numbers (not that they mean a lot here)
            var fullXml = $"<identifier>{innerXml}</identifier>";
            return ParseIdentifierString<T>(fullXml, typeMap);
        }

        private static T ParseIdentifierString<T>(string fullXml, Dictionary<string, Type> typeMap)
        {
            var xDocument = XDocument.Load(
                XmlReader.Create(new StringReader(fullXml)),
                LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);

            var identifier = XmlParser.ParseIdentifier(xDocument.Root, typeMap);

            Assert.NotNull(identifier);
            return Assert.IsType<T>(identifier);
        }

        [Fact]
        public void CanParseSexIdentifier()
        {
            var xml = @"<mdwy:sex xmlns:mdwy=""uk.nhs.bradfordhosptials.bib4all.medway"">Male</mdwy:sex>";
            
            var sexIdentifier = ParseIdentifierString<UkNhsBradfordhospitalsBib4allMedwaySex>(
                xml,
                new Dictionary<string, Type>
                {
                    ["uk.nhs.bradfordhosptials.bib4all.medway.sex"] = typeof(UkNhsBradfordhospitalsBib4allMedwaySex) 
                }
                );
            
            Assert.Equal(Sex.Male.ToString(), sexIdentifier.Sex);
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
            
            var identifier = ParseIdentifierString<DummyIdentifierWithOneArg<Sex>>(
                xml,
                new Dictionary<string, Type>
                {
                    ["dummy-identifier"] = typeof(DummyIdentifierWithOneArg<Sex>) 
                }
            );
            
            Assert.Equal(Sex.Male, identifier.Value);
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
           
            var dateofBirthIdentifier = ParseIdentifier<UkNhsBradfordhospitalsBib4allMedwayDateOfBirth>("2016-03-14");
            AssertDateOfBirth(14.March(2016), dateofBirthIdentifier);
        }

        [Fact]
        public void CanParseNameIdentifier()
        {
            var nameIdentifier = ParseIdentifier<UkNhsBradfordhospitalsBib4allMedwayName>("<firstName>Bart</firstName><lastName>Simpson</lastName>");
            AssertName("Bart", "Simpson", nameIdentifier);
        }

        [Fact]
        public void CanParsePartialAddress()
        {
            var address =
                ParseIdentifier<UkNhsBradfordhospitalsBib4allMedwayAddress>(
                    "<addressLine1>line 1</addressLine1><postcode>T3 3ST</postcode>");
            
            AssertAddress(address, line1:"line 1", postcode:"T3 3ST");
        }

        [Fact]
        public void ReportsErrorOnIncorrectType()
        {
            var parseException = Assert.ThrowsAny<XmlParseException>(() => ParseIdentifier<UkNhsNhsNumber>("", EmptyTypeMap));

            Assert.Contains("identifier", parseException.Message);
            Assert.True(parseException.HasLineInfo);
            Assert.Equal(1, parseException.LineNumber);
            Assert.Equal(2, parseException.LinePosition);
            
        }

        [Fact]
        public void CanParseWholePerson()
        {
            const string personXml = @"<?xml version=""1.0""?>
<people xmlns:nhs=""uk.nhs"" xmlns:bfd=""uk.nhs.bradfordhospitals"" xmlns:mdw=""uk.nhs.bradfordhospitals.bib4all.medway"">
    <person>
        <identity>
            <nhs:nhsNumber>4099999999</nhs:nhsNumber>
            <bfd:hospitalNumber>RAE9999999</bfd:hospitalNumber>
            <mdw:name><firstName>Jo</firstName><lastName>Bloggs</lastName></mdw:name>
            <mdw:dateOfBirth>1990-06-16</mdw:dateOfBirth>
            <mdw:address>
                <addressLine1>22 Love Street</addressLine1>
                <addressLine2>Holmestown</addressLine2>
                <addressLine3>Bradtopia</addressLine3>
                <addressLine4>West Yorkshire</addressLine4>
                <postcode>BD92 4FX</postcode>
            </mdw:address>
            <mdw:contactNumber>
                <type>Home</type>
                <number>01234999999</number>
            </mdw:contactNumber>
            <mdw:contactNumber>
                <type>Mobile</type>
                <number>01234999999</number>
            </mdw:contactNumber>
        </identity>
        <lookup>
            <match><nhs:nhsNumber>4099999999</nhs:nhsNumber></match>
            <match><bfd:hospitalNumber>RAE9999999</bfd:hospitalNumber></match>
        </lookup>
        <consent dateGiven=""2017-10-14"" studyId=""TBC"">
            <givenFor>
                <mdw:pregnancyNumber>2</mdw:pregnancyNumber>
            </givenFor>
            <evidence>
                <mdw:evidence>
                    <competentStatus></competentStatus>
                    <givenBy></givenBy>
                    <takenBy>Betsey Trotwood</takenBy>
                </mdw:evidence>
            </evidence>
        </consent>
    </person>
</people>";

            var xmlTextReader = XmlReader.Create(new StringReader(personXml), new XmlReaderSettings {IgnoreWhitespace = true});
            var person = new XmlParser().GetPeople(xmlTextReader).Single();

            Action<IPersonIdentifier> Address(
                string line1 = null,
                string line2 = null,
                string line3 = null,
                string line4 = null,
                string line5 = null,
                string postcode = null) =>
                AssertIdentifier<UkNhsBradfordhospitalsBib4allMedwayAddress>(
                    _ => AssertAddress(_, line1, line2, line3, line4, line5, postcode));
            

//            Assert.DoesNotContain(person.Identifiers, _ => _ == null);
            Assert.Collection(person.Identifiers,
                NhsNumber("4099999999"),
                HosptialNumber("RAE9999999"),
                Name("Jo", "Bloggs"),
                DateOfBirth(16.June(1990)),
                Address("22 Love Street", "Holmestown", "Bradtopia", "West Yorkshire", postcode: "BD92 4FX"),
                ContactNumber("Home", "01234999999"),
                ContactNumber("Mobile", "01234999999")
                );
            
            Assert.Collection(person.MatchSpecifications,
                m => Assert.Collection(m.Identifiers, NhsNumber("4099999999")),
                m => Assert.Collection(m.Identifiers, HosptialNumber("RAE9999999"))
            );
        }

        private static Action<IPersonIdentifier> NhsNumber(string expectedValue) => AssertIdentifier<UkNhsNhsNumber>(
            i => AssertNhsNumber(expectedValue, i));

        private static void AssertNhsNumber(string expectedValue, UkNhsNhsNumber identifier) => Assert.Equal(expectedValue, identifier.Value);

        private static Action<IPersonIdentifier> HosptialNumber(string expectedValue) => AssertIdentifier<UkNhsBradfordhospitalsHospitalNumber>(
            i => AssertHosptialNumber(expectedValue, i));

        private static void AssertHosptialNumber(string expectedValue, UkNhsBradfordhospitalsHospitalNumber i) => Assert.Equal(expectedValue, i.Value);

        private static Action<IPersonIdentifier> DateOfBirth(DateTime dateofBirth) => AssertIdentifier<UkNhsBradfordhospitalsBib4allMedwayDateOfBirth>(
            i => AssertDateOfBirth(dateofBirth, i));

        private static void AssertDateOfBirth(DateTime dateofBirth, UkNhsBradfordhospitalsBib4allMedwayDateOfBirth i) => Assert.Equal(dateofBirth, i.DateOfBirth);

        private static Action<IPersonIdentifier> Name(string firstName, string lastName) => AssertIdentifier<UkNhsBradfordhospitalsBib4allMedwayName>(
            _ => { AssertName(firstName, lastName, _); });

        private static void AssertName(string firstName, string lastName, UkNhsBradfordhospitalsBib4allMedwayName _)
        {
            Assert.Equal(firstName, _.FirstName);
            Assert.Equal(lastName, _.LastName);
        }

        private static Action<IPersonIdentifier> ContactNumber(string type, string number) => AssertIdentifier<UkNhsBradfordhospitalsBib4allMedwayContactNumber>(
            _ => { AssertContactNumber(type, number, _); });

        private static void AssertContactNumber(string type, string number, UkNhsBradfordhospitalsBib4allMedwayContactNumber _)
        {
            Assert.Equal(type, _.Type);
            Assert.Equal(number, _.Number);
        }

        private static Action<IPersonIdentifier> AssertIdentifier<T>(Action<T> test) => _ => test(Assert.IsType<T>(_));

        private void AssertAddress(
            UkNhsBradfordhospitalsBib4allMedwayAddress address,
            string line1 = null,
            string line2 = null,
            string line3 = null,
            string line4 = null,
            string line5 = null,
            string postcode = null)
        {
            Assert.Equal(line1, address.AddressLine1);
            Assert.Equal(line2, address.AddressLine2);
            Assert.Equal(line3, address.AddressLine3);
            Assert.Equal(line4, address.AddressLine4);
            Assert.Equal(line5, address.AddressLine5);
            Assert.Equal(postcode, address.Postcode);
        }
    }
}