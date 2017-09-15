using System.Collections.Generic;
using System.Linq.Expressions;
using System.Xml;
using System.Xml.Linq;
using CHC.Consent.Common.Evidence;
using CHC.Consent.Common.Identity;

namespace CHC.Consent.Common.Import.Datasources
{
    public class XmlStandardDataReader : StandardDataReader
    {
        public const string DefaultNamespace = "urn:chc:consent:v0.1";
        private static readonly XNamespace ChcNs = DefaultNamespace; 
        private static readonly XNamespace Xsi = "http://www.w3.org/2001/XMLSchema-instance";
        private readonly FileDatasource datasource;

        public XmlStandardDataReader(FileDatasource datasource)
        {
            this.datasource = datasource;
        }

        public IEnumerable<IPerson> People()
        {
            using (var reader = CreateReader())
            {
                reader.MoveToContent();
                reader.ReadToDescendant("person", DefaultNamespace);

                while (reader.IsStartElement("person", DefaultNamespace))
                {
                    var me = XNode.ReadFrom(reader) as XElement;

                    if (me == null)
                    {
                        //TODO: Error logging
                        continue;
                    }

                    var person = new XmlPerson();

                    foreach (var identityElement in me.Elements(ChcNs+"identities").Elements())
                    {
                        if (identityElement.Name == ChcNs + "simpleIdentity")
                        {
                            var identityKind = new IdentityKind{ Id = identityElement.Element(ChcNs + "identityKindId").Value};
                            person.Identities.Add(
                                identityKind,
                                new SimpleIdentity
                                {
                                    IdentityKind = identityKind,
                                    Value = identityElement.Element(ChcNs + "value").Value
                                }
                            );
                        }
                        else
                        {
                            //TODO: composite identity
                            //TODO: error logging
                            continue;
                        }
                    }

                    yield return person;
                }
            }
        }

        protected virtual XmlReader CreateReader()
        {
            return new XmlTextReader(datasource.FileLocation);
        }
    }
    
    public class XmlPerson : IPerson
    {
        public IDictionary<IdentityKind, Identity.Identity> Identities { get; } = new Dictionary<IdentityKind, Identity.Identity>();
        public IDictionary<EvidenceKind, Evidence.Evidence> Evidence { get; } = new Dictionary<EvidenceKind, Evidence.Evidence>();
    }
}