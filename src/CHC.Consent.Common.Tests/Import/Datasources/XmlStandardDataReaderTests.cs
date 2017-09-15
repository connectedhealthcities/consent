using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Import;
using CHC.Consent.Common.Import.Datasources;
using Xunit;
using Xunit.Abstractions;

namespace CHC.Consent.Common.Tests.Import.Datasources
{
    public class XmlStandardDataReaderTests
    {
        private static readonly XNamespace Ns = XmlStandardDataReader.ChcStandardDataNamespace;
        
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
            var person = ReadPeople(new XElement(Ns + "person")).First();
            
            Assert.Empty(person.Identities);
        }
        
        [Fact]
        public void ReadsTwoPersonsWithNoIdentity()
        {
            var person = ReadPeople(
                new XElement(Ns + "person"),
                new XElement(Ns + "person"));
            
            Assert.Empty(person[0].Identities);
            Assert.Empty(person[1].Identities);
        }
        
        [Fact]
        public void ReadsOnePersonWithSimpleIdentity()
        {

            var person = ReadPeople(
                new XElement(
                    Ns + "person",
                    new XElement(
                        Ns + "identities",
                        new XElement(
                            Ns + "simpleIdentity",
                            new XElement(Ns + "identityKindId", "id"),
                            new XElement(Ns + "value", "value")
                        )
                    ))).First();
            
            
            
            
            Assert.NotEmpty(person.Identities);
            var identity = person.Identities.Single();

            Assert.Equal("id", identity.Key.Id);
            Assert.IsType<SimpleIdentity>(identity.Value);
            Assert.Equal("value", ((SimpleIdentity) identity.Value).Value);
        }
        

        private IPerson[] ReadPeople(params XNode[] personElements)
        {
            var people = new XmlStandardDataReaderFromString(
                new XDocument(
                    new XElement(
                        Ns + "people",
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

            public XmlStandardDataReaderFromString(XNode source, ITestOutputHelper output) : base(new FileDatasource(string.Empty))
            {
                this.source = source;
                this.output = output;
            }

            protected override XmlReader CreateReader()
            {
                output.WriteLine(source.ToString());
                var xmlReaderSettings = new XmlReaderSettings();
                xmlReaderSettings.Schemas.Add(ChcStandardDataNamespace, "StandardData.xsd");
                return XmlReader.Create(
                    source.CreateReader(),
                    xmlReaderSettings);
                
            }
        }
    }
}