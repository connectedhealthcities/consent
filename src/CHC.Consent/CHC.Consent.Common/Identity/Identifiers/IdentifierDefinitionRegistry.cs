using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.Common.Infrastructure.Definitions;

namespace CHC.Consent.Common.Identity.Identifiers
{
    public class IdentifierDefinitionRegistry : DefinitionRegistry, IEnumerable<IdentifierDefinition>
    {
        /// <inheritdoc />
        public IdentifierDefinitionRegistry()
        {
        }

        /// <inheritdoc />
        public IdentifierDefinitionRegistry(IEnumerable<IdentifierDefinition> definitions):base(definitions) 
        {
        }

        public IdentifierDefinitionRegistry(params IdentifierDefinition[] definitions) : this(definitions.AsEnumerable())
        {
        }


        /// <inheritdoc />
        IEnumerator<IdentifierDefinition> IEnumerable<IdentifierDefinition>.GetEnumerator() =>
            Items.Cast<IdentifierDefinition>().GetEnumerator();
    }
}