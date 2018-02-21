using System;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity.Identifiers;

namespace CHC.Consent.EFCore.Entities
{
    public class NameEntity : NameIdentifier
    {
        public long Id { get; private set; }
        public PersonEntity Person { get; set; }
        
        /// <inheritdoc />
        public NameEntity() 
        {
        }
    }
}