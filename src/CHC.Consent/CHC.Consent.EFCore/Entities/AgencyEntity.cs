using System;
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
        protected AgencyEntity()
        {
        }

        /// <inheritdoc />
        public AgencyEntity(string name, string systemName)
        {
            Name = name;
            SystemName = systemName;
        }

        public string Name { get; protected set; }
        public string SystemName { get; protected set; }

        public ICollection<AgencyFieldEntity> Fields { get; protected set; } = new List<AgencyFieldEntity>();

        /// <inheritdoc />
        public long Id { get; protected set; }

        public static implicit operator Agency(AgencyEntity entity)
        {
            if (entity == null) return null;
            return new Agency
            {
                Id = (AgencyIdentity) entity.Id,
                Name = entity.Name,
                SystemName = entity.SystemName,
                Fields =
                    entity.Fields?.Select(
                            _ =>
                                string.Join(
                                    IdentifierSearch.Separator,
                                    new[] {DefinitionParser.ParseString(_.Identifier.Definition).Name, _.Subfields}
                                        .Where(name => !string.IsNullOrWhiteSpace(name))))
                        .ToArray() ?? Array.Empty<string>()
            };
        }
    }
}