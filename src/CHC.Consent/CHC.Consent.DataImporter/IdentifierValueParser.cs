using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CHC.Consent.Api.Client.Models;

namespace CHC.Consent.DataImporter
{
    public class IdentifierValueParser
    {
        public IdentifierValueParser(
            IDictionary<string, IdentifierDefinition> definitions)
        {
            Definitions = definitions;
            Parsers = definitions.ToDictionary(x => x.Key, x => CreateValueParser(x.Value));
        }

        private IReadOnlyDictionary<string, Func<XElement, object>> Parsers { get; }
        private IDictionary<string, IdentifierDefinition> Definitions { get; set; }

        public IdentifierValue Parse(XElement identifierNode)
        {
            var typeName = identifierNode.Attribute("type")?.Value;
            if (!Definitions.ContainsKey(typeName))
            {
                throw new XmlParseException(identifierNode, $"Cannot parse identifier of Type '{typeName}'");
            }

            return new IdentifierValue
            {
                Name = Definitions[typeName].SystemName,
                Value = Parsers[typeName](identifierNode)
            };
        }
        
        public static Func<XElement, object> CreateValueParser(IdentifierDefinition definition)
        {
            switch (definition.Type)
            {
                case CompositeIdentifierType composite:
                    var parsers = composite.Identifiers.ToDictionary(e => e.Key, e => CreateValueParser(e.Value));
                    var valueParser = new IdentifierValueParser(composite.Identifiers);
                    return x =>
                        //TODO: Handle extra elements - warning or errors?
                        x.Elements()
                            .Select(valueParser.Parse)
                            .ToArray();
                case DateIdentifierType date:
                    return x => (DateTime)x;
                case EnumIdentifierType @enum:
                    return x =>
                        @enum.Values.FirstOrDefault(
                            v => v.Equals(x.Value, StringComparison.InvariantCultureIgnoreCase)) ??
                        throw new NotImplementedException($"Don't know what to do with enum value of {x.Value}");
                case IntegerIdentifierType integer:
                    return x => (int) x;
                case StringIdentifierType @string:
                    return x => x.Value; 
            }

            throw new NotImplementedException(
                $"Don't know how to handle Identifiers of type {definition.Type.SystemName}");
        }
    }
}