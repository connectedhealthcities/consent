using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Api.Client.Models;
using CsvHelper;

namespace CHC.Consent.DataImporter.Features.ExportData
{
    internal class ValueOutputFormatter : IDefinitionVisitor<IdentifierDefinition>
    {
        private IEnumerable<IdentifierDefinition> IdentifierDefinitions { get; }

        /// <inheritdoc />
        public ValueOutputFormatter(IEnumerable<IdentifierDefinition> identifierDefinitions)
        {
            IdentifierDefinitions = identifierDefinitions.ToArray();
            this.VisitAll(IdentifierDefinitions);
        }

        private delegate void Writer(IIdentifierValueDto dto, IWriterRow writer);

        private IDictionary<string, Writer> Writers { get; } = new Dictionary<string, Writer>();

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, CompositeIdentifierType type)
        {
            var compositeDefinitions = type.Identifiers.Cast<IdentifierDefinition>().ToArray();
            var subWriters = new ValueOutputFormatter(compositeDefinitions);
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
            (dto, writer) => writer.WriteField(((IdentifierValueDtoDateTime) dto)?.Value);

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
            IWriterRow output)
        {
            var valuesByName = identifiers.ToDictionary(_ => _.Name);
            foreach (var identifierDefinition in IdentifierDefinitions)
            {
                valuesByName.TryGetValue(identifierDefinition.SystemName, out var identifier);
                Writers[identifierDefinition.SystemName](identifier, output);
            }
        }
    }
}