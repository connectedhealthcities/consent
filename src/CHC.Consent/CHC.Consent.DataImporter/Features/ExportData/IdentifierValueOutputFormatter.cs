using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Api.Client.Models;
using CsvHelper;

namespace CHC.Consent.DataImporter.Features.ExportData
{
    internal class IdentifierValueOutputFormatter : IDefinitionVisitor<IdentifierDefinition>
    {
        /// <inheritdoc />
        public IdentifierValueOutputFormatter(
            IEnumerable<IdentifierDefinition> definitions, IEnumerable<string[]> fieldNames)
        {
            FieldNames = fieldNames.ToLookup(_ => _.First(), _ => _.Skip(1).ToArray());
            var topLevelFieldNames = FieldNames.Select(_ => _.Key).ToArray();
            IdentifierDefinitions = GetDefinitionsInOutputOrder(definitions, topLevelFieldNames);

            CreateWriterForDefinitions();
        }

        private ILookup<string, string[]> FieldNames { get; }
        private IEnumerable<IdentifierDefinition> IdentifierDefinitions { get; }

        private IDictionary<string, Writer> Writers { get; } = new Dictionary<string, Writer>();

        private static Writer WriteDate { get; } =
            (dto, writer) => writer.WriteField(((IdentifierValueDtoDateTime) dto)?.Value?.ToString("yyyy-MM-dd"));

        private static Writer WriteEnum { get; } =
            (dto, writer) => writer.WriteField(((IdentifierValueDtoString) dto)?.Value);

        private static Writer WriteInteger { get; } =
            (dto, writer) => writer.WriteField(((IdentifierValueDtoInt64) dto)?.Value);

        private static Writer WriteString { get; } =
            (dto, writer) => writer.WriteField(((IdentifierValueDtoString) dto)?.Value);

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, CompositeDefinitionType type)
        {
            var subWriters = new IdentifierValueOutputFormatter(
                type.Identifiers.Cast<IdentifierDefinition>(),
                FieldNames[definition.SystemName]
            );

            Writers[definition.SystemName] = subWriters.WriteCompositeValue;
        }

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, DateDefinitionType type)
        {
            Writers[definition.SystemName] = WriteDate;
        }

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, EnumDefinitionType type)
        {
            Writers[definition.SystemName] = WriteEnum;
        }


        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, IntegerDefinitionType type)
        {
            Writers[definition.SystemName] = WriteInteger;
        }

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, StringDefinitionType type)
        {
            Writers[definition.SystemName] = WriteString;
        }

        private static IEnumerable<IdentifierDefinition> GetDefinitionsInOutputOrder(
            IEnumerable<IdentifierDefinition> definitions,
            IEnumerable<string> topLevelFieldNames)
        {
            return topLevelFieldNames.Select(definitions.GetDefinition).ToArray();
        }

        private void CreateWriterForDefinitions()
        {
            this.VisitAll(IdentifierDefinitions);
        }

        private void WriteCompositeValue(IIdentifierValueDto dto, IWriterRow writer)
        {
            var compositeDto = (IdentifierValueDtoIIdentifierValueDto) dto;
            var subValues = compositeDto?.Value ?? Enumerable.Empty<IIdentifierValueDto>();

            Write(subValues, writer);
        }

        public void Write(IEnumerable<IIdentifierValueDto> identifierValues, IWriterRow destination)
        {
            //TODO: Handle multiple values
            var values = new IdentifierValueWrapper(identifierValues);
            foreach (var identifierDefinition in IdentifierDefinitions)
            {
                var fieldName = identifierDefinition.SystemName;

                var writer = GetWriter(fieldName);

                writer(values.GetValue(fieldName), destination);
            }
        }

        private Writer GetWriter(string fieldName)
        {
            return Writers[fieldName];
        }

        private delegate void Writer(IIdentifierValueDto dto, IWriterRow writer);

        private class IdentifierValueWrapper
        {
            private readonly Dictionary<string, IIdentifierValueDto> valuesByName;

            public IdentifierValueWrapper(IEnumerable<IIdentifierValueDto> identifiers)
            {
                valuesByName = identifiers.ToLookup(_ => _.Name).ToDictionary(_ => _.Key, _ => _.First());
            }

            public IIdentifierValueDto GetValue(string fieldName)
            {
                valuesByName.TryGetValue(fieldName, out var value);
                return value;
            }
        }
    }
}