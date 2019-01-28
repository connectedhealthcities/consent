using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Api.Client.Models;

namespace CHC.Consent.DataImporter.Features.ExportData
{
    internal class FieldNames : IDefinitionVisitor<IdentifierDefinition>, IEnumerable<string>
    {
        public const string Separator = "::";

        public static string Join(IdentifierDefinition definition, string compositeName) =>
            Join(definition.SystemName, compositeName);

        public static string Join(params string[] parts) => string.Join(Separator, parts);

        /// <inheritdoc />
        public FieldNames(IEnumerable<IdentifierDefinition> definitions)
        {
            this.VisitAll(definitions);
        }

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, CompositeIdentifierType type)
        {
            var compositeNames = new FieldNames(type.Identifiers.Cast<IdentifierDefinition>());
            foreach (var compositeName in compositeNames)
            {
                Names.Add(Join(definition, compositeName));
            }
        }

        public ICollection<string> Names { get;  } = new HashSet<string>();

        private void AddName(IdentifierDefinition definition) => Names.Add($"{definition.SystemName}");

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, DateIdentifierType type) => AddName(definition);

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, EnumIdentifierType type) => AddName(definition);
        
        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, IntegerIdentifierType type) => AddName(definition);
        
        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, StringIdentifierType type) => AddName(definition);
        
        /// <inheritdoc />
        public IEnumerator<string> GetEnumerator() => Names.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) Names).GetEnumerator();
    }
}