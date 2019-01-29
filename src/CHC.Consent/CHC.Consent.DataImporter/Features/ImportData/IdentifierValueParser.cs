using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml.Linq;
using CHC.Consent.Api.Client.Models;

namespace CHC.Consent.DataImporter.Features.ImportData
{
    public class IdentifierValueParser
    {
        public static IdentifierValueParser CreateFrom<T>(IEnumerable<T> definitions) where T:IDefinition, IAcceptDefinitionVisitor<T>
        {
            var parsers = new ValueParserCreator<T>().VisitAll(definitions).Parsers;
            
            return new IdentifierValueParser(definitions, parsers);
        }

        private IdentifierValueParser(IEnumerable<IDefinition> definitions, IDictionary<string, Parser> parsers)
        {
            Definitions = definitions.ToDictionary(x => x.SystemName);
            Parsers = parsers.ToImmutableDictionary();
        }

        private delegate IIdentifierValueDto Parser(XElement element);
        private IReadOnlyDictionary<string, Parser> Parsers { get; }
        private IDictionary<string, IDefinition> Definitions { get; set; }

        public IIdentifierValueDto Parse(XElement identifierNode)
        {
            var typeName = identifierNode.Attribute("type")?.Value;
            if (!Definitions.ContainsKey(typeName))
            {
                throw new XmlParseException(identifierNode, $"Cannot parse identifier of Type '{typeName}'");
            }

            return Parsers[typeName](identifierNode);
        }
        
        private class ValueParserCreator<TDefinition> : IDefinitionVisitor<TDefinition> 
            where TDefinition : IDefinition, IAcceptDefinitionVisitor<TDefinition>
        {
            public IDictionary<string, Parser> Parsers { get; } = new Dictionary<string, Parser>();
            
            private delegate IIdentifierValueDto UnnamedParser(string name, XElement element);
            private void SetParser(TDefinition definition, UnnamedParser parser) =>
                Parsers[definition.SystemName] = x =>  parser(definition.SystemName, x);
            
            /// <inheritdoc />
            public void Visit(TDefinition definition, CompositeDefinitionType type)
            {
                var valueParser = CreateFrom(type.Identifiers.Cast<TDefinition>());
                SetParser(
                    definition,
                    (name, x) =>
                        new IdentifierValueDtoIIdentifierValueDto(
                            name,
                            x.Elements()
                                .Select(valueParser.Parse)
                                .ToArray()));
            }

            /// <inheritdoc />
            public void Visit(TDefinition definition, DateDefinitionType type)
            {
                SetParser(definition, (name, x) => new IdentifierValueDtoDateTime(name, (DateTime)x));
            }

            /// <inheritdoc />
            public void Visit(TDefinition definition, EnumDefinitionType type)
            {
                SetParser(
                    definition,
                    (name, x) =>
                        new IdentifierValueDtoString(
                            name,
                            @type.Values.FirstOrDefault(
                                v => v.Equals(x.Value, StringComparison.InvariantCultureIgnoreCase)) ??
                            throw new NotImplementedException($"Don't know what to do with enum value of {x.Value}")
                        ));
            }

            /// <inheritdoc />
            public void Visit(TDefinition definition, IntegerDefinitionType type)
            {
                SetParser(definition, (name,x) => new IdentifierValueDtoInt64(name, (long) x));
            }

            /// <inheritdoc />
            public void Visit(TDefinition definition, StringDefinitionType type)
            {
                SetParser(definition, (name, x) => new IdentifierValueDtoString(name, x.Value));
            }
        }
    }
}