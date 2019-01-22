using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CHC.Consent.Api.Client.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

namespace CHC.Consent.DataImporter
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
            private EvidenceDefinition definition;

            /// <inheritdoc />
            public ImportFileEvidenceGenerator(IEnumerable<EvidenceDefinition> evidenceDefinitions)
            {
                definition = evidenceDefinitions.FirstOrDefault(_ => _.SystemName == "import-file");
            }

            public IEnumerable<IIdentifierValueDto> GenerateFrom(XElement element)
            {
                var lineInfo = (IXmlLineInfo) element;


                if(definition == null) yield break;
                if(!(definition.Type is CompositeIdentifierType compositeIdentifierType)) yield break;

                var innerDefinitions = compositeIdentifierType?.Identifiers;

                var baseUri = innerDefinitions.FirstOrDefault(_ => _.SystemName == "base-uri");
                var lineNumber = innerDefinitions.FirstOrDefault(_ => _.SystemName == "line-number");
                var linePosition = innerDefinitions.FirstOrDefault(_ => _.SystemName == "line-position");
                
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
        
        private ILogger Log { get; } = NullLogger.Instance;
        
        private readonly IdentifierValueParser identifierValueParser;
        private readonly IdentifierValueParser evidenceValueParser;
        private ImportFileEvidenceGenerator importFileEvidenceGenerator;

        public XmlParser(
            ILogger<XmlParser> logger, 
            IList<IdentifierDefinition> identifierDefinitions,
            IList<EvidenceDefinition> evidenceDefinitions)
        {
            Log = logger;

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
                //TODO: Record errors
                throw new NotImplementedException();
            }

            while (xmlReader.Read())
            {
                if (xmlReader.NodeType != XmlNodeType.Element || xmlReader.LocalName != "person")
                {
                    //TODO: record errors
                    continue;
                }


                var personNode = XDocument.Load(xmlReader.ReadSubtree(), LoadOptions.SetLineInfo|LoadOptions.SetBaseUri);

                var person = new PersonSpecification
                {
                    Identifiers = personNode.XPathSelectElements("/person/identity/identifier")
                        .Select(ParseIdentifier)
                        .ToArray(),
                    MatchSpecifications =
                        personNode.XPathSelectElements("/person/lookup/match")
                            .Select(ParseMatchSpecification)
                            .ToArray()
                };

                var consentSpecifications = personNode.XPathSelectElements("/person/consent")
                    .Select(_ => ParseConsent(_))
                    .ToArray();

                //TODO: Catch, record, log, and return details of exceptions 

                yield return new ImportedPersonSpecification
                {
                    PersonSpecification = person,
                    ConsentSpecifications = consentSpecifications
                };
            }
        }

        public ImportedConsentSpecification ParseConsent(
            XElement consentNode) 
        {
            var date = (DateTime)consentNode.Attribute("date-given");
            var studyId = (long)consentNode.Attribute("study-id");

            //TODO: better error recording and typey stuff here

            var evidence = consentNode.XPathSelectElements("evidence/evidence")
                .Select(evidenceValueParser.Parse)
                .ToArray();
            
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
            return identifierValueParser.Parse(identifierNode);
        }


        private static object ChangeType(string stringValue, Type type) => 
            type.IsEnum
            ? Enum.Parse(type, stringValue)
            : Convert.ChangeType(stringValue, type);

        private static readonly Regex LowerCaseFollowedByUpperCase = new Regex(
            @"(\p{Ll})(\p{Lu})",
            RegexOptions.Compiled);
    }
}