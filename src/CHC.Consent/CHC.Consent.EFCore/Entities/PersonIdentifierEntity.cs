using System;
using CHC.Consent.Common.Infrastructure.Data;

namespace CHC.Consent.EFCore.Entities
{
    public class PersonIdentifierEntity : IEntity
    {
        public long Id { get; protected set; }
        public PersonEntity Person { get; set; }
        public string TypeName { get; set; }
        public string Value { get; set; }
        public string ValueType { get; set; }
        public DateTime Created { get; protected set; } = DateTime.UtcNow;
        public DateTime? Deleted { get; set; }
    }
}