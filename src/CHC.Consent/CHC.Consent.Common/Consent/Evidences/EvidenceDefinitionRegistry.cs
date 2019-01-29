using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure.Definitions;

namespace CHC.Consent.Common.Consent.Evidences
{
    public class EvidenceDefinitionRegistry : DefinitionRegistry
    {
        /// <inheritdoc />
        public EvidenceDefinitionRegistry()
        {
        }

        /// <inheritdoc />
        public EvidenceDefinitionRegistry(IEnumerable<EvidenceDefinition> definitions) 
        {
            foreach (var definition in definitions)
            {
                Add(definition);
            }
        }

        public EvidenceDefinitionRegistry(params EvidenceDefinition[] definitions) : this(definitions.AsEnumerable())
        {
        }
    }
}