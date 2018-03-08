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
        private static T ParseIdentifier<T>(string innerXml)
        {
            var xml = XElement.Parse(
                $"<identifier type=\"identifier\">{innerXml}</identifier>");

            var identifier = XmlParser.ParseIdentifier(
                xml,
                new Dictionary<string, Type> {["identifier"] = typeof(T)});
            
            Assert.NotNull(identifier);
            return Assert.IsType<T>(identifier);
        }
        
        [Fact]
        public void CanParseSexIdentifier()
        {
            var sexIdentifier = ParseIdentifier<UkNhsBradfordhospitalsBib4allMedwaySex>("Male");
            
            Assert.Equal(Sex.Male.ToString(), sexIdentifier.Sex);
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
        public void CanParseWholePerson()
        {
            const string personXml = @"<people><person><!-- mother example -->
                            <identity>
                <identifier type=""uk.nhs.nhs-number"">4099999999</identifier>
                <identifier type=""uk.nhs.bradfordhospitals.hospital-number"">RAE9999999</identifier>
                <identifier type=""uk.nhs.bradfordhospitals.bib4all.medway.name""><firstName>Jo</firstName><lastName>Bloggs</lastName></identifier>
                <identifier type=""uk.nhs.bradfordhospitals.bib4all.medway.date-of-birth"">1990-06-16</identifier>
                <identifier type=""uk.nhs.bradfordhospitals.bib4all.medway.address"">
                <addressLine1>22 Love Street</addressLine1>
                <addressLine2>Holmestown</addressLine2>
                <addressLine3>Bradtopia</addressLine3>
                <addressLine4>West Yorkshire</addressLine4>
                <addressLine5></addressLine5><!-- should this be empty or excluded? -->
                <postcode>BD92 4FX</postcode>
                </identifier>
                <identifier type=""uk.nhs.bradfordhospitals.bib4all.medway.contact-number"">
                <type>Home</type>
                <number>01234999999</number>
                </identifier>
                <identifier type=""uk.nhs.bradfordhospitals.bib4all.medway.contact-number"">
                <type>Mobile</type>
                <number>01234999999</number>
                </identifier>
                </identity>
                <lookup>
                <match><identifier type=""uk.nhs.nhs-number"">4099999999</identifier></match>
                <match><identifier type=""uk.nhs.bradfordhospitals.hospital-number"">RAE9999999</identifier></match>
                </lookup>
                <consent dateGiven=""2017-10-14"" studyId=""TBC"">
                <givenFor>
                <discriminator type=""uk.nhs.bradfordhospitals.bib4all.medway.pregnancyNumber"">2</discriminator>
                </givenFor>
                <evidence type=""uk.nhs.bradfordhospitals.bib4all.medway.evidence"">
                <competentStatus></competentStatus>
                <givenBy></givenBy>
                <takenBy>Betsey Trotwood</takenBy>
                </evidence>
                </consent>
                </person></people>";

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