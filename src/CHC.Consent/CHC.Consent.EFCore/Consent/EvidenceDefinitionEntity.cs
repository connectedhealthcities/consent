using System;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace CHC.Consent.EFCore.Consent
{
    public class EvidenceDefinitionEntity : IEntity, IDefinitionEntity
    {
        /// <inheritdoc />
        public long Id { get; set; }

        public string Name { get; set; }
        public string Definition { get; set; }
    }
}