using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Api.Client.Models;

namespace CHC.Consent.DataImporter.Features.ExportData
{
    internal class GatherOutputNames : IDefinitionVisitor<IdentifierDefinition>, IEnumerable<string>
    {
        /// <inheritdoc />
        public GatherOutputNames(IEnumerable<IdentifierDefinition> definitions)
        {
            this.VisitAll(definitions);
        }

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, CompositeIdentifierType type)
        {
            var compositeNames = new GatherOutputNames(type.Identifiers.Cast<IdentifierDefinition>());
            foreach (var compositeName in compositeNames)
            {
                Names.Add($"{definition.SystemName}::{compositeName}");   
            }
        }

        public ICollection<string> Names { get;  } = new HashSet<string>();

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, DateIdentifierType type)
        {
            Names.Add($"{definition.SystemName}");
        }

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, EnumIdentifierType type)
        {
            Names.Add($"{definition.SystemName}");
        }

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, IntegerIdentifierType type)
        {
            Names.Add($"{definition.SystemName}");
        }

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, StringIdentifierType type)
        {
            Names.Add($"{definition.SystemName}");
        }

        /// <inheritdoc />
        public IEnumerator<string> GetEnumerator() => Names.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) Names).GetEnumerator();
    }
}