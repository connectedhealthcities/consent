using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using CHC.Consent.Api.Client.Models;
using CHC.Consent.DataImporter.Features.ExportData;
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

        private static string[] Export(
            IEnumerable<CHC.Consent.Common.Identity.Identifiers.IdentifierDefinition> identifiers, 
            string[] fieldNames = default,
            params StudySubjectWithIdentifiers[] people)
        {
            string result;
            using (var output = new StringWriter())
            {
                new CsvExporter(null, fieldNames)
                    .Write(
                    identifiers.Select(_ => _.ConvertToClientDefinition()).ToImmutableArray(),
                    people,
                    output);

                result = output.ToString();
            }

            var strings = result.Split(new[] {"\n", "\r\n"}, StringSplitOptions.None);
            return strings;
        }

        private static StudySubjectWithIdentifiers Person(
            string subjectIdentifier,
            string firstName = null,
            string lastname = null,
            DateTime? dateOfBirth = null)
        {
            var identifiers = new List<IIdentifierValueDto>();

            if (firstName != null || lastname != null)
            {
                var nameParts = new List<IIdentifierValueDto>();
                if(firstName != null) nameParts.Add(KnownIdentifiers.FirstName.Value(firstName));
                if(lastname != null) nameParts.Add(KnownIdentifiers.LastName.Value(lastname));

                identifiers.Add(KnownIdentifiers.Name.Value(nameParts));
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