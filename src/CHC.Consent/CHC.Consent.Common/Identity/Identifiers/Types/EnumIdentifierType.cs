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
        }

        /// <inheritdoc />
        public void Accept(IIdentifierDefinitionVisitor visitor)
        {
            visitor.Visit(this);
        }

        /// <inheritdoc />
        public IdentifierParseResult Parse(string value) => IdentifierParseResult.Success(value);
        
    }
}