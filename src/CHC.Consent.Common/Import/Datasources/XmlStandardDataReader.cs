using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CHC.Consent.Common.Import.Match;
using CHC.Consent.Identity.SimpleIdentity;
using CHC.Consent.Import.Core;

namespace CHC.Consent.Common.Import.Datasources
{
    public class XmlStandardDataReader : StandardDataReader
    {
        public static class X
        {
            private static readonly XNamespace ChcNs = XmlNames.ChcNs;
            
            public static readonly XName People = ChcNs + "people";
            public static readonly XName Person = ChcNs + "person";
            public static readonly XName Identities = ChcNs + "identities";

            public static readonly XName SimpleIdentity = ChcNs + "simpleIdentity";
            public static readonly XName IdentityKindId = ChcNs + "identityKindId";

            public static readonly XName MatchIdenty = ChcNs + "matchIdentity";
            public static readonly XName MatchStudyIdentity = ChcNs + "matchStudyIdentity";
            public static readonly XName Match = ChcNs + "match";
            public static readonly XName Evidence = ChcNs + "evidence";
            public static readonly XName EvidenceKindId = ChcNs + "evidenceKindId";
        }
        
        private readonly FileDatasource datasource;
        
        public XmlStandardDataReader(FileDatasource datasource)
        {
            this.datasource = datasource;
        }

        public override IEnumerable<IImportRecord> People()
        {
            using (var reader = CreateReader())
            {
                reader.MoveToContent();
                reader.ReadToDescendant("person", XmlNames.ChcStandardDataNamespace);

                while (reader.IsStartElement("person", XmlNames.ChcStandardDataNamespace))
                {
                    var currentNode = XNode.ReadFrom(reader) as XElement;

                    if (currentNode == null)
                    {
                        //TODO: Error logging
                        continue;
                    }

                    var person = new XmlImportRecord();

                    person.Identities.AddRange(ReadIdentities(currentNode));
                    
                    person.MatchIdentity = ReadMatches(currentNode.Element(X.MatchIdenty)).ToArray();
                    person.MatchStudyIdentity = ReadIdentityElements(currentNode.Element(X.MatchStudyIdentity)).ToArray();

                    person.Evidence.AddRange(ReadEvidence(currentNode.Element(X.Evidence)));

                    yield return person;
                }
            }
        }

        private IEnumerable<EvidenceRecord> ReadEvidence(XElement element)
        {
            if(element == null) yield break;
            foreach (var evidenceElement in element.Elements())
            {
                if (evidenceElement.Name == X.Evidence)
                {
                    // TODO: check descendents of evidence element
                    yield return new EvidenceRecord
                    {
                        EvidenceKindExternalId = evidenceElement.Element(X.EvidenceKindId)?.Value,
                        Evidence = evidenceElement.Element(X.Evidence).Value
                    };
                }
                else
                {
                    // TODO: error handling/recording
                }
                
            }
        }

        private IEnumerable<MatchRecord> ReadMatches(XContainer matchIdentities)
        {
            if (matchIdentities == null)
            {
                //TODO: error logging
                yield break;
            }
            foreach (var matchElement in matchIdentities.Elements(X.Match))
            {
                foreach (var matchRecord in ReadIdentityElements(matchElement)) yield return matchRecord;
            }
        }

        private static IEnumerable<MatchByIdentityKindIdRecord> ReadIdentityElements(XElement matchElement)
        {
            if (matchElement == null) return Enumerable.Empty<MatchByIdentityKindIdRecord>();
            //TODO logging of wrong elements  
            return
                from logicElement in matchElement.Elements()
                where IsIdentityKindId(logicElement)
                select
                    new MatchByIdentityKindIdRecord
                    {
                        IdentityKindExternalId = logicElement.Value
                    };
        }

        private static bool IsIdentityKindId(XElement logicElement)
        {
            return logicElement.Name == X.IdentityKindId;
        }

        private IEnumerable<IdentityRecord> ReadIdentities(XContainer person)
        {
            foreach (var identityElement in person.Elements(X.Identities).Elements())
            {
                if (identityElement.Name == X.SimpleIdentity)
                {
                    //TODO: Validate simple identity from xml - external kind id and value 
                    var identityKindExternalId = identityElement.Element(X.IdentityKindId).Value;
                    yield return new SimpleIdentityKindProvider().ReadXml(identityKindExternalId, identityElement);
                }
                else
                {
                    //TODO: handle reading composite identity
                    //TODO: error logging for reading identites
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