using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using CHC.Consent.Common.Core;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Import;
using CHC.Consent.Common.Import.Datasources;
using CHC.Consent.Common.Import.Match;
using Xunit;
using Xunit.Abstractions;
 

namespace CHC.Consent.Common.Tests.Import.Datasources
{
    using X = XmlStandardDataReader.X;
    public class XmlStandardDataReaderTests
    {
        private readonly ITestOutputHelper output;

        public XmlStandardDataReaderTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void AnEmptyFileHasNoPeople()
        {
            Assert.Empty(ReadPeople());
        }


        [Fact]
        public void ReadsOnePersonWithNoIdentity()
        {
            var person = ReadPeople(XPerson()).First();
            
            Assert.Empty(person.Identities);
        }
        
        [Fact]
        public void ReadsTwoPersonsWithNoIdentity()
        {
            var person = ReadPeople(XPerson(),XPerson());
            
            Assert.Empty(person[0].Identities);
            Assert.Empty(person[1].Identities);
        }
        
        [Fact]
        public void ReadsOnePersonWithSimpleIdentity()
        {
            var xml = XPerson(identities: XIdentities(XSimpleIdentity("id", "value")));
            var person = ReadPeople(xml).First();
            
            Assert.NotEmpty(person.Identities);
            var identity = person.Identities.Single();

            Assert.Equal("id", identity.IdentityKindId);
            Assert.IsType<SimpleIdentityRecord>(identity);
            Assert.Equal("value", ((SimpleIdentityRecord) identity).Value);
        }

        [Fact]
        public void ReadsSimpleMatchCorrectly()
        {
            var xPerson = XPerson(
                XIdentities(XSimpleIdentity("id", Guid.NewGuid().ToString())),
                XMatchIdentity(XIdentityKindId("id")),
                XMatchStudyIdentity(XIdentityKindId("id"))
            );

            var person = ReadPeople(xPerson).First();
            var matchIdentity = person.MatchIdentity.First();
            var matchStudyIdentity = person.MatchStudyIdentity.First(); 

            Assert.IsType<IdentityKindId>(matchIdentity );
            Assert.Equal("id", ((IdentityKindId)matchIdentity).Id);

            Assert.IsType<IdentityKindId>(matchStudyIdentity);
            Assert.Equal("id", ((IdentityKindId)matchStudyIdentity).Id);
        }


        private static XElement XPerson(
            XElement identities = null, XElement matchIdentity = null, XElement matchStudyIdentity = null)
        {
            var person = new XElement(X.Person);
            if(identities != null)
                person.Add(identities);
            if(matchIdentity != null)
                person.Add(matchIdentity);
            if(matchStudyIdentity != null)
                person.Add(matchStudyIdentity);

            return person;
        }

        private static XElement XIdentityKindId(string id)
        {
            return new XElement(X.IdentityKindId, id);
        }
        
        private static XElement XMatchStudyIdentity(params XElement[] matches)
        {
            return matches.Any() ? new XElement(X.MatchStudyIdentity,  matches.Select(_ => new XElement(X.Match, _))) : null;
        }

        private static XElement XMatchIdentity(params XElement[] matches)
        {
            return matches.Any() ? new XElement(X.MatchIdenty, matches.Select(_ => new XElement(X.Match, _))) : null;
        }
        
        private static XElement XIdentities(params XElement[] identities)
        {
            return identities.Any() ? new XElement(X.Identities, identities) : null;
        }
        
        private static XElement XSimpleIdentity(string kindId, string value, string id = null)
        {
            var xSimpleIdentity = new XElement(
                X.SimpleIdentity,
                new XElement(X.IdentityKindId, kindId),
                new XElement(X.Value, value)
            );
            if (id != null)
            {
                xSimpleIdentity.AddFirst(new XAttribute("id", id));
            }
            return xSimpleIdentity;
        }


        private IImportRecord[] ReadPeople(params XNode[] personElements)
        {
            var people = new XmlStandardDataReaderFromString(
                new XDocument(
                    new XElement(
                        X.People,
                        personElements.Cast<object>().ToArray()
                    )
                ),
                output
            ).People().ToArray();
            return people;
        }


        class XmlStandardDataReaderFromString : XmlStandardDataReader
        {
            private readonly XNode source;
            private readonly ITestOutputHelper output;

            public XmlStandardDataReaderFromString(XNode source, ITestOutputHelper output) : base(new FileDatasource(string.Empty, new Study()))
            {
                this.source = source;
                this.output = output;
            }

            protected override XmlReader CreateReader()
            {
                output.WriteLine(source.ToString());
                var xmlReaderSettings = new XmlReaderSettings();
                xmlReaderSettings.Schemas.Add(ChcStandardDataNamespace, "StandardData.xsd");
                xmlReaderSettings.Schemas.Compile();
                xmlReaderSettings.XmlResolver = new XmlUrlResolver();
                xmlReaderSettings.ValidationType = ValidationType.Schema;
                xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings
                                                     | XmlSchemaValidationFlags.ProcessIdentityConstraints
                                                     | XmlSchemaValidationFlags.ProcessInlineSchema
                                                     | XmlSchemaValidationFlags.ProcessSchemaLocation;
                xmlReaderSettings.ValidationEventHandler += (sender, args) =>
                {
                    output.WriteLine("{1}: Error reading xml: {0}", args.Message, args.Severity);
                };
                return XmlReader.Create(
                    source.CreateReader(),
                    xmlReaderSettings);
                
            }
        }
    }
}