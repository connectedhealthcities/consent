using System;
using System.Collections.Generic;
using System.IO;
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
            private ICollection<IdentifierDefinition> identifiers;
            private StudySubjectWithIdentifiers[] people;
            private readonly string result;
            private readonly string[] lines;

            /// <inheritdoc />
            public WhenExportingAllIdentifiers()
            {
                identifiers = new[]
                {
                    
                    KnownIdentifiers.Name.ConvertToClientDefinition(), 
                    KnownIdentifiers.DateOfBirth.ConvertToClientDefinition()
                };
                people = new[]
                {
                    Person("001", "Helen", "Davies", 15.June(2016)),
                    Person("002", dateOfBirth:18.July(2017)),
                    Person("003", lastname:"Richardson"),
                    Person("004", firstName:"Alistair"),
                    Person("005"),
                };

                using (var output = new StringWriter())
                {
                    new CsvExporter(null).Write(identifiers, people, output);

                    result = output.ToString();
                    
                }

                lines = result.Split(new[] {"\n", "\r\n"}, StringSplitOptions.None);

            }

            [Fact]
            public void WritesCorrectNumberOfLines()
            {
                lines.Should().HaveCount(people.Length + 2); // one for header and blank line at end
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
}