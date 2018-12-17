using System;

namespace CHC.Consent.Common.Identity.Identifiers
{
    public class DateIdentifierType : IIdentifierType
    {
        /// <inheritdoc />
        public virtual void Accept(IIdentifierDefinitionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}