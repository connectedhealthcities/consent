using System;
using System.Collections.Generic;

namespace CHC.Consent.Common.Identity.Identifiers
{
    public class EnumIdentifierType : IIdentifierType
    {
        public ISet<string> Values { get; }

        /// <inheritdoc />
        public EnumIdentifierType(params string[] values)
        {
            Values = new HashSet<string>(values, StringComparer.InvariantCultureIgnoreCase);
            SystemName = $"enum({string.Join(",", Values)})";
        }


        /// <inheritdoc />
        public void Accept(IDefinitionVisitor visitor, IDefinition definition) =>
            visitor.Visit(definition, this);

        /// <inheritdoc />
        public string SystemName { get; }

        /// <inheritdoc />
        public IdentifierParseResult Parse(string value) => IdentifierParseResult.Success(value);
        
    }
}