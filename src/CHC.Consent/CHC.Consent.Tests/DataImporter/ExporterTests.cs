using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using CHC.Consent.Api.Client.Models;
using CHC.Consent.DataTool.Features.ExportData;
using CHC.Consent.Testing.Utils;
using FluentAssertions;
using Xunit;


namespace CHC.Consent.Tests.DataImporter
{
    using KnownIdentifiers = Identifiers.Definitions;
    public class ExporterTests
    {
        public class WhenExportingAllIdentifiers
        {
            private readonly string[] lines;

            /// <inheritdoc />
            public WhenExportingAllIdentifiers()
            {
                lines = Export(
                    new[] { KnownIdentifiers.Name, KnownIdentifiers.DateOfBirth },
                    new [] { "name::given", "name::family", "date-of-birth" },
                    Person("001", "Helen", "Davies", 15.June(2016)),
                    Person("002", dateOfBirth: 18.July(2017)),
                    Person("003", lastname: "Richardson"),
                    Person("004", firstName: "Alistair"),
                    Person("005"));
            }

            [Fact]
            public void WritesCorrectNumberOfLines()
            {
                lines.Should().HaveCount(7); // one for header, one for each person, and blank line at end
            }

            [Fact]
            public void WritesCorrectHeader()
            {
                lines[0].Should().Be("id,name::given,name::family,date-of-birth");
            }

            [Fact]
            public void WritesCompletePerson()
            {
                lines[1].Should().Be("001,Helen,Davies,2016-06-15");
            }
            
            [Fact]
            public void WritesOnlyDateOfBirth()
            {
                lines[2].Should().Be("002,,,2017-07-18");
            }
            
            [Fact]
            public void WritesOnlyLastName()
            {
                lines[3].Should().Be("003,,Richardson,");
            }
            
            [Fact]
            public void WritesOnlyFirstname()
            {
                lines[4].Should().Be("004,Alistair,,");
            }

            [Fact]
            public void WriteNoIdentifiers()
            {
                lines[5].Should().Be("005,,,");
            }
        }

        [Fact]
        public void WhenExportingNotIdentifiers_OnlyIdsAreWritten()
        {
            Export(new[] {KnownIdentifiers.Name, KnownIdentifiers.DateOfBirth, KnownIdentifiers.Address}, 
                    fieldNames:Array.Empty<string>(),
                Person("001", "Ignored", "Ignored"))
                .Should()
                .BeEquivalentTo("id", "001", "");
        }
        
        
        [Fact]
        public void WhenExportingSimpleFields_SelectedFieldsAreWritten()
        {
            Export(new[] {KnownIdentifiers.Name, KnownIdentifiers.DateOfBirth, KnownIdentifiers.Address}, 
                    fieldNames:new [] { KnownIdentifiers.DateOfBirth.SystemName },
                    Person("001", "Ignored", "Ignored", 27.April(2014)))
                .Should()
                .BeEquivalentTo("id,date-of-birth", "001,2014-04-27", "");
        }
        
        [Fact]
        public void WhenExportingNestedFields_SelectedFieldsAreWritten()
        {
            Export(new[] {KnownIdentifiers.Name, KnownIdentifiers.DateOfBirth, KnownIdentifiers.Address}, 
                    fieldNames:new [] { "name::family" },
                    Person("001", "Ignored", "Surname", 27.April(2014)))
                .Should()
                .BeEquivalentTo("id,name::family", "001,Surname", "");
        }
        
        [Fact]
        public void WhenExportingFields_FieldsAreWrittenInCorrectOrder()
        {
            Export(
                    new[] {KnownIdentifiers.Address, KnownIdentifiers.DateOfBirth, KnownIdentifiers.Name,},
                    fieldNames: new[] {"name::given", "name::family", "address::postcode"},
                    Person(
                        "001",
                        "First name",
                        "Surname",
                        27.April(2014),
                        ClientIdentifierValues.Address(postcode: "SN13 7HG")
                    )
                )
                .Should()
                .BeEquivalentTo(
                    "id,name::given,name::family,address::postcode",
                    "001,First name,Surname,SN13 7HG",
                    ""
                );
        }

        private static string[] Export(
            IEnumerable<CHC.Consent.Common.Identity.Identifiers.IdentifierDefinition> identifiers, 
            IEnumerable<string> fieldNames,
            params StudySubjectWithIdentifiers[] people)
        {
            string result;
            using (var output = new StringWriter())
            {
                new StudySubjectCsvWriter(() => output)
                    .Write(
                        identifiers.Select(_ => _.ConvertToClientDefinition()).ToImmutableArray(),
                        FieldNameList.Split(fieldNames),
                        people);

                result = output.ToString();
            }

            var strings = result.Split(new[] {"\n", "\r\n"}, StringSplitOptions.None);
            return strings;
        }
        
        

        private static StudySubjectWithIdentifiers Person(
            string subjectIdentifier,
            string firstName = null,
            string lastname = null,
            DateTime? dateOfBirth = null,
            params IIdentifierValueDto[] otherIdentifiers
            )
        {
            var identifiers = new List<IIdentifierValueDto>(otherIdentifiers);

            if (firstName != null || lastname != null)
            {
                identifiers.Add(ClientIdentifierValues.Name(firstName, lastname));
            }

            if(dateOfBirth != null) identifiers.Add(KnownIdentifiers.DateOfBirth.Value(dateOfBirth.Value));
                
            return new StudySubjectWithIdentifiers
            {
                subjectIdentifier = subjectIdentifier,
                identifiers = identifiers
            };
        }
    }
}