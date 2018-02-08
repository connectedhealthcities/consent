using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Api.Features.Identity.Dto;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;

namespace CHC.Consent.Api.Features.Identity
{
    public class ParsedPersonSpecification
    {
        public IReadOnlyList<Identifier> Identifiers { get; }

        private IDictionary<string, Identifier> IdentifiersById { get; }
        
        private PersonSpecification OriginalSpecification { get; }
        

        public ParsedPersonSpecification(
            IReadOnlyList<Identifier> identifiers, 
            IDictionary<string, Identifier> identifiersById,
            PersonSpecification originalSpecification)
        {
            Identifiers = identifiers;
            IdentifiersById = identifiersById;
            OriginalSpecification = originalSpecification;
        }

        private Identifier GetIdentifierById(string identifierId)
        {
            if (IdentifiersById.TryGetValue(identifierId, out var identifier))
            {
                return identifier;
            }

            throw new InvalidOperationException($"Cannot find identifier with id '{identifierId}'");
        }

        private IEnumerable<Identifier> GetIdentifiersByExternalId(string externalId)
        {
            var matchIdentifiers =
                Identifiers
                    .Where(_ => _.IdentifierType.ExternalId == externalId)
                    .ToArray();

            if (matchIdentifiers.Any()) return matchIdentifiers;
            
            throw new InvalidOperationException($"Cannot find Identifier with Type {externalId}");
        }

        private IEnumerable<Identifier> IdentifiersForMatch(MatchIdentifierSpecification matchIdentifier)
        {
            switch (matchIdentifier.MatchBy)
            {
                case MatchBy.Id:
                    return Enumerable.Repeat( GetIdentifierById(matchIdentifier.IdOrType), 1);
                case MatchBy.Type:
                    return GetIdentifiersByExternalId(matchIdentifier.IdOrType);
                default:
                    throw new ArgumentOutOfRangeException($"Cannot match by '{matchIdentifier.MatchBy}'");
            }
        }


        private IEnumerable<Identifier> GetIdentifiersToMatch(MatchSpecification specification)
        {
            return specification.Identifiers.SelectMany(IdentifiersForMatch).ToArray();
        }

        public delegate Person PersonByIdentifierFinder(IEnumerable<Identifier> identifiers);

        public IEnumerable<IEnumerable<Identifier>> Matches()
        {
            return OriginalSpecification.MatchSpecifications.Select(GetIdentifiersToMatch);
        }
    }
}