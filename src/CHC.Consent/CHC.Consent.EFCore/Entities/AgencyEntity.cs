using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Identity;
using CHC.Consent.Parsing;

namespace CHC.Consent.EFCore.Entities
{
    public class AgencyEntity : IEntity
    {
        private static readonly IdentifierDefinitionParser DefinitionParser = new IdentifierDefinitionParser();

        /// <inheritdoc />
        public long Id { get; protected set; }
        public string Name { get; protected set; }
        public string SystemName { get; protected set; }
        
        public ICollection<AgencyFieldEntity> Fields { get; protected set; } = new List<AgencyFieldEntity>();

        /// <inheritdoc />
        protected AgencyEntity()
        {
        }

        /// <inheritdoc />
        public AgencyEntity(string name, string systemName)
        {
            Name = name;
            SystemName = systemName;
        }

        public static implicit operator Agency(AgencyEntity entity)
        {
            if (entity == null) return null;
            return new Agency
            {
                Id = (AgencyIdentifier) entity.Id,
                Name = entity.Name,
                SystemName = entity.SystemName,
                Fields = entity.Fields?.Select(_ => DefinitionParser.ParseString(_.Identifier.Definition).Name)
                             .ToArray() ?? Array.Empty<string>()
            };
        }
    }
}