using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Api.Client.Models;
using CsvHelper;

namespace CHC.Consent.DataImporter.Features.ExportData
{
    internal class ValueOutputFormatter : IDefinitionVisitor<IdentifierDefinition>
    {
        private ILookup<string, string[]> fieldNames;
        private IEnumerable<IdentifierDefinition> IdentifierDefinitions { get; }

        /// <inheritdoc />
        public ValueOutputFormatter(IEnumerable<IdentifierDefinition> identifierDefinitions, IEnumerable<string[]> fieldNames)
        {
            IdentifierDefinitions = identifierDefinitions.ToArray();
            this.fieldNames = fieldNames.ToLookup(_ => _.First(), _ => _.Skip(1).ToArray());
            this.VisitAll(IdentifierDefinitions);
        }

        private delegate void Writer(IIdentifierValueDto dto, IWriterRow writer);

        private IDictionary<string, Writer> Writers { get; } = new Dictionary<string, Writer>();

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, CompositeIdentifierType type)
        {
            var subfieldNames = fieldNames[definition.SystemName].ToArray();
            var compositeLevelFieldNames = subfieldNames.Select(_ => _.First()).ToArray();
            var compositeDefinitions = 
                type.Identifiers.Cast<IdentifierDefinition>()
                    .Where(d => compositeLevelFieldNames.Contains(d.SystemName))
                    .ToArray();

            var subWriters = new ValueOutputFormatter(compositeDefinitions, subfieldNames);
            Writers[definition.SystemName] = delegate(IIdentifierValueDto dto, IWriterRow writer)
            {
                var compositeDtos =
                    ((IdentifierValueDtoIIdentifierValueDto) dto)?.Value ??
                    Enumerable.Empty<IIdentifierValueDto>();

                subWriters.Write(compositeDtos, writer);
            };
        }

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, DateIdentifierType type)
        {
            Writers[definition.SystemName] = WriteDate;
        }

        private static Writer WriteDate { get; } =
            (dto, writer) => writer.WriteField(((IdentifierValueDtoDateTime) dto)?.Value?.ToString("yyyy-MM-dd"));

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, EnumIdentifierType type)
        {
            Writers[definition.SystemName] =
                WriteEnum;
        }

        private static Writer WriteEnum { get; } =
            (dto, writer) => writer.WriteField(((IdentifierValueDtoString) dto)?.Value);


        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, IntegerIdentifierType type)
        {
            Writers[definition.SystemName] = WriteInteger;
        }

        private static Writer WriteInteger { get; } = 
            (dto, writer) => writer.WriteField(((IdentifierValueDtoInt64) dto)?.Value);

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, StringIdentifierType type)
        {
            Writers[definition.SystemName] = WriteString;
        }

        private static Writer WriteString { get; } =
            (dto, writer) => writer.WriteField(((IdentifierValueDtoString) dto)?.Value);

        public void Write(
            IEnumerable<IIdentifierValueDto> identifiers,
            IWriterRow destination)
        {
            var valuesByName = identifiers.ToDictionary(_ => _.Name);
            foreach (var identifierDefinition in IdentifierDefinitions)
            {
                valuesByName.TryGetValue(identifierDefinition.SystemName, out var identifier);
                Writers[identifierDefinition.SystemName](identifier, destination);
            }
        }
    }
}