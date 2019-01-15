using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CHC.Consent.Api.Client.Models;

namespace CHC.Consent.DataImporter
{
    public class IdentifierValueParser
    {
        public IdentifierValueParser(IList<IdentifierDefinition> definitions)
        {
            Definitions = definitions.ToDictionary(x => x.SystemName);
            Parsers = definitions.ToDictionary(x => x.SystemName, CreateValueParser);
        }

        private delegate IIdentifierValueDto Parser(XElement element);
        private IReadOnlyDictionary<string, Parser> Parsers { get; }
        private IDictionary<string, IdentifierDefinition> Definitions { get; set; }

        public IIdentifierValueDto Parse(XElement identifierNode)
        {
            var typeName = identifierNode.Attribute("type")?.Value;
            if (!Definitions.ContainsKey(typeName))
            {
                throw new XmlParseException(identifierNode, $"Cannot parse identifier of Type '{typeName}'");
            }


            return Parsers[typeName](identifierNode);

        }

        private static Parser CreateValueParser(IdentifierDefinition definition)
        {
            var name = definition.SystemName;
            switch (definition.Type)
            {
                case CompositeIdentifierType composite:
                    var parsers = composite.Identifiers.ToDictionary(e => e.SystemName, CreateValueParser);
                    var valueParser = new IdentifierValueParser(composite.Identifiers);
                    return x =>
                        new IdentifierValueDtoIIdentifierValueDto(
                            name,
                            x.Elements()
                                .Select(valueParser.Parse)
                                .ToArray());
                case DateIdentifierType date:
                    return x => new IdentifierValueDtoDateTime(name, (DateTime)x);
                case EnumIdentifierType @enum:
                    return x =>
                        new IdentifierValueDtoString(name, 
                        @enum.Values.FirstOrDefault(
                            v => v.Equals(x.Value, StringComparison.InvariantCultureIgnoreCase)) ??
                        throw new NotImplementedException($"Don't know what to do with enum value of {x.Value}")
                        );
                case IntegerIdentifierType integer:
                    return x => new IdentifierValueDtoInt64(name, (long) x);
                case StringIdentifierType @string:
                    return x => new IdentifierValueDtoString(name, x.Value); 
            }

            throw new NotImplementedException(
                $"Don't know how to handle Identifiers of type {definition.Type.SystemName}");
        }
    }
}