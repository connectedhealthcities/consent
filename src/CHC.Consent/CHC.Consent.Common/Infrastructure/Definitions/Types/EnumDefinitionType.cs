using System;
using System.Collections.Generic;

namespace CHC.Consent.Common.Infrastructure.Definitions.Types
{
    public class EnumDefinitionType : IDefinitionType
    {
        public ISet<string> Values { get; }

        /// <inheritdoc />
        public EnumDefinitionType(params string[] values)
        {
            Values = new HashSet<string>(values, StringComparer.InvariantCultureIgnoreCase);
            SystemName = $"enum({string.Join(",", Values)})";
        }


        /// <inheritdoc />
        public void Accept(IDefinitionVisitor visitor, IDefinition definition) =>
            visitor.Visit(definition, this);

        /// <inheritdoc />
        public string SystemName { get; }
    }
}