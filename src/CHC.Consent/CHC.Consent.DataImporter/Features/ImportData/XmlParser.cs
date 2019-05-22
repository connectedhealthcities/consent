using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CHC.Consent.Api.Client.Models;
using Serilog;
using Serilog.Context;

namespace CHC.Consent.DataImporter.Features.ImportData
{
    /// <summary>
    /// <para>Simple XML parser that tries to match element names against constructor arguments.</para>
    /// <para>This will always find the constructor with most arguments and try to apply that.</para>
    /// <para>It honours default argument values. In the case of only one argument, the contents of the node is used</para>
    /// </summary>
    public class XmlParser
    {
        private class ImportFileEvidenceGenerator
        {
            private readonly EvidenceDefinition definition;

            /// <inheritdoc />
            public ImportFileEvidenceGenerator(IEnumerable<EvidenceDefinition> evidenceDefinitions)
            {
                definition = evidenceDefinitions.FirstOrDefault(_ => _.SystemName == "import-file");
            }

            public IEnumerable<IIdentifierValueDto> GenerateFrom(XElement element)
            {
                var lineInfo = (IXmlLineInfo) element;


                if(definition == null) yield break;
                if(!(definition.Type is CompositeDefinitionType composite)) yield break;

                var fields = composite.Identifiers;

                var baseUri = fields.FirstOrDefault(_ => _.SystemName == "base-uri");
                var lineNumber = fields.FirstOrDefault(_ => _.SystemName == "line-number");
                var linePosition = fields.FirstOrDefault(_ => _.SystemName == "line-position");
                
                var innerValues = new List<IIdentifierValueDto>();
                if (baseUri != null && !string.IsNullOrWhiteSpace(element.BaseUri))
                    innerValues.Add(new IdentifierValueDtoString("base-uri", element.BaseUri));
                
                if (lineInfo.HasLineInfo())
                {
                    if(lineNumber != null) innerValues.Add(new IdentifierValueDtoInt64(lineNumber.SystemName, lineInfo.LineNumber));
                    if(linePosition != null) innerValues.Add(new IdentifierValueDtoInt64(linePosition.SystemName, lineInfo.LinePosition));
                }
                
                yield return new IdentifierValueDtoIIdentifierValueDto(definition.SystemName, innerValues);

            }
        }

        private readonly IdentifierValueParser identifierValueParser;
        private readonly IdentifierValueParser evidenceValueParser;
        private readonly ImportFileEvidenceGenerator importFileEvidenceGenerator;
        private ILogger Log { get; } = Serilog.Log.ForContext<XmlParser>();

        public XmlParser(
            IList<IdentifierDefinition> identifierDefinitions,
            IList<EvidenceDefinition> evidenceDefinitions)
        {
            importFileEvidenceGenerator = new ImportFileEvidenceGenerator(evidenceDefinitions);
            evidenceValueParser = IdentifierValueParser.CreateFrom(evidenceDefinitions);
            identifierValueParser = IdentifierValueParser.CreateFrom(identifierDefinitions);
        }

        public IEnumerable<ImportedPersonSpecification> GetPeople(StreamReader source)
        {
            var xmlReader = XmlReader.Create(source);

            return GetPeople(xmlReader);
        }

        public IEnumerable<ImportedPersonSpecification> GetPeople(XmlReader xmlReader)
        {
            xmlReader = XmlReader.Create(
                xmlReader,
                new XmlReaderSettings {IgnoreWhitespace = true, IgnoreComments = true});

            xmlReader.MoveToContent();

            if (xmlReader.LocalName != "people")
            {
                Log.Fatal(
                    "Expected people element but found {@element} at {@lineInfo}",
                    new {xmlReader.Name, xmlReader.NodeType},
                    LineInfo(xmlReader));
                //TODO: Record errors
                throw new NotImplementedException();
            }

            
            Authority = xmlReader["authority"];
            if (Authority == null)
            {
                Log.Fatal("people element has no authority attribute at {@lineInfo}", LineInfo(xmlReader));
                throw new NotImplementedException($"people element has no authority attribute");
            }

            while (xmlReader.Read())
            {
                if (IsEndOfPeople(xmlReader)) break;
                var lineInfo = LineInfo(xmlReader);
                using(LogContext.PushProperty("FileLocation",lineInfo))
                {
                    foreach (var importedPersonSpecification in ParsePeople(xmlReader))
                        yield return importedPersonSpecification;
                }
            }
        }

        public string Authority { get; private set; }

        private static object LineInfo(XmlReader xmlReader)
        {
            var xmlLineInfo = (IXmlLineInfo) xmlReader;
            return new {xmlLineInfo.LineNumber, xmlLineInfo.LinePosition};
        }

        private bool IsEndOfPeople(XmlReader xmlReader)
        {
            return xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.LocalName == "people";
        }

        private IEnumerable<ImportedPersonSpecification> ParsePeople(XmlReader xmlReader)
        {
            if (xmlReader.NodeType != XmlNodeType.Element)
            {
                Log.Error("Don't know how to handle {name} {type}", xmlReader.Name, xmlReader.NodeType);
                throw new NotImplementedException();
            }
            if (xmlReader.LocalName != "person")
            {
                Log.Error("Don't know how to handle {name} {type}", xmlReader.Name, xmlReader.NodeType);
                //skip this element and subtree
                xmlReader.ReadSubtree().Dispose();
                yield break;
            }


            var personNode = XDocument.Load(xmlReader.ReadSubtree(), LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);

            var person = new PersonSpecification
            {
                Identifiers = personNode.XPathSelectElements("/person/identity/identifier")
                    .Select(ParseIdentifier)
                    .ToArray(),
                Authority = Authority,
                MatchSpecifications =
                    personNode.XPathSelectElements("/person/lookup/match")
                        .Select(ParseMatchSpecification)
                        .ToArray()
            };

            var consentSpecifications = personNode.XPathSelectElements("/person/consent")
                .Select(ParseConsent)
                .ToArray();

            //TODO: Catch, record, log, and return details of exceptions 

            yield return new ImportedPersonSpecification
            {
                PersonSpecification = person,
                ConsentSpecifications = consentSpecifications
            };
        }

        public ImportedConsentSpecification ParseConsent(
            XElement consentNode) 
        {
            var date = (DateTime)consentNode.Attribute("date-given");
            var studyId = (long)consentNode.Attribute("study-id");

            //TODO: better error recording and typey stuff here

            var evidence = ParseEvidence(consentNode);

            if (evidence.Any())
            {
                evidence = evidence.Concat(GetImportSourceEvidence(consentNode)).ToArray();
            }

            var givenBy = consentNode.XPathSelectElements("givenBy/match")
                .Select(ParseMatchSpecification)
                .ToArray();

            return new ImportedConsentSpecification
            {
                DateGiven = date,
                StudyId = studyId,
                Evidence = evidence,
                GivenBy = givenBy
            };
        }

        private IIdentifierValueDto[] ParseEvidence(XElement consentNode)
        {
            using (LogContext.PushProperty("DefinitionType", "Evidence"))
            {
               return consentNode.XPathSelectElements("evidence/evidence")
                    .Select(evidenceValueParser.Parse)
                    .ToArray();
            }

            
        }

        private IEnumerable<IIdentifierValueDto> GetImportSourceEvidence(XElement node)
        {
            return importFileEvidenceGenerator.GenerateFrom(node);

        }

        private MatchSpecification ParseMatchSpecification(XContainer node)
        {
            return new MatchSpecification(node.Elements("identifier").Select(ParseIdentifier).ToList());
        }


        public IIdentifierValueDto ParseIdentifier(XElement identifierNode)
        {
            using (LogContext.PushProperty("DefinitionType", "Identifier"))
            {
                return identifierValueParser.Parse(identifierNode);
            }
        }
    }
}