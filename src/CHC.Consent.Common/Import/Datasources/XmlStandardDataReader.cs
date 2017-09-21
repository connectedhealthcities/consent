using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml;
using System.Xml.Linq;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Import.Match;

namespace CHC.Consent.Common.Import.Datasources
{
    public class XmlStandardDataReader : StandardDataReader
    {
        public const string ChcStandardDataNamespace = "urn:chc:consent:standard-data:v0.1";
        

        public static class X
        {
            private static readonly XNamespace ChcNs = ChcStandardDataNamespace;
            
            public static readonly XName People = ChcNs + "people";
            public static readonly XName Person = ChcNs + "person";
            public static readonly XName Identities = ChcNs + "identities";

            public static readonly XName SimpleIdentity = ChcNs + "simpleIdentity";
            public static readonly XName IdentityKindId = ChcNs + "identityKindId";

            public static readonly XName Value = ChcNs + "value";

            public static readonly XName MatchIdenty = ChcNs + "matchIdentity";
            public static readonly XName MatchStudyIdentity = ChcNs + "matchStudyIdentity";
            public static readonly XName Match = ChcNs + "match";
        }
        
        private readonly FileDatasource datasource;
        

        public XmlStandardDataReader(FileDatasource datasource)
        {
            this.datasource = datasource;
        }

        public override IEnumerable<IPerson> People()
        {
            using (var reader = CreateReader())
            {
                reader.MoveToContent();
                reader.ReadToDescendant("person", ChcStandardDataNamespace);

                while (reader.IsStartElement("person", ChcStandardDataNamespace))
                {
                    var me = XNode.ReadFrom(reader) as XElement;

                    if (me == null)
                    {
                        //TODO: Error logging
                        continue;
                    }

                    var person = new XmlPerson();

                    foreach (var (identityKind, identity) in ReadIdentities(me))
                    {
                        person.Identities.Add(identityKind, identity);
                    }

                    person.MatchIdentity = ReadMatches(me.Element(X.MatchIdenty)).ToArray();
                    person.MatchStudyIdentity = ReadMatches(me.Element(X.MatchStudyIdentity)).ToArray();

                    yield return person;
                }
            }
        }

        private IEnumerable<Match.Match> ReadMatches(XContainer matchIdentities)
        {
            if (matchIdentities == null)
            {
                //TODO: error logging
                yield break;
            }
            foreach (var matchElement in matchIdentities.Elements(X.Match))
            {
                foreach (var logicElement in matchElement.Elements())
                {
                    if (logicElement.Name == X.IdentityKindId)
                    {
                        yield return new IdentityKindId{Id = logicElement.Value};
                    }
                }
            }
        }

        private IEnumerable<(IdentityKind identityKind, Identity.Identity identity)> ReadIdentities(XContainer person)
        {
            foreach (var identityElement in person.Elements(X.Identities).Elements())
            {
                if (identityElement.Name == X.SimpleIdentity)
                {
                    //TODO: more validation on these
                    var identityKind = new IdentityKind{ ExternalId = identityElement.Element(X.IdentityKindId).Value};
                    var identity = new SimpleIdentity
                    {
                        IdentityKind = identityKind,
                        Value = identityElement.Element(X.Value).Value
                    };
                    yield return (identityKind,identity);
                }
                else
                {
                    //TODO: composite identity
                    //TODO: error logging
                    continue;
                }
            }
        }

        protected virtual XmlReader CreateReader()
        {
            return new XmlTextReader(datasource.FileLocation);
        }
    }
}