using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CHC.Consent.Api.Features.Identity.Dto;
using CHC.Consent.Api.Infrastructure;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;

namespace CHC.Consent.Api.Features.Identity
{
    public class PersonSpecificationParser
    {
        private readonly IIdentifierTypeRegistry identifierTypes;


        public PersonSpecificationParser(IIdentifierTypeRegistry identifierTypes)
        {
            this.identifierTypes = identifierTypes;
        }

        public ParsedPersonSpecification Parse(PersonSpecification specification)
        {
            var identifiersAndIds = 
                specification.Identifiers
                    .Select(idSpec => (ReferenceId: idSpec.Id, Identifier: ParseIdentifier(idSpec)))
                .ToArray();
            
            var identifiers = identifiersAndIds.Select(_ => _.Identifier).ToArray();
            identifiers.EnsureHasNoInvalidDuplicates();

            var identifiersWithIds = identifiersAndIds.Where(_ => !string.IsNullOrEmpty(_.ReferenceId)).ToArray();
            EnsureNoDupliateReferenceIds(identifiersWithIds);

            var identifiersById = identifiersWithIds.ToDictionary(_ => _.ReferenceId, _ => _.Identifier); 
        

            return new ParsedPersonSpecification(identifiers, identifiersById, specification);
        }

        private static void EnsureNoDupliateReferenceIds(
            IEnumerable<(string ReferenceId, Identifier Identifier)> identifiersWithIds)
        {
            var duplicateReferenceIds = identifiersWithIds.GroupBy(_ => _.ReferenceId)
                .Where(_ => _.Count() > 1)
                .Select(_ => _.Key)
                .ToArray();
            if (!duplicateReferenceIds.Any()) return;
            
            throw new InvalidOperationException(
                new StringBuilder("Duplicate Ids specified: ").AppendJoin(',', duplicateReferenceIds).ToString());
        }

        private Identifier ParseIdentifier(IdentifierSpecification identifierSpecification)
        {
            var identifierType = identifierSpecification.Type;
            if (identifierType == null)
            {
                throw new InvalidOperationException($"Identifier Type not specified for {identifierSpecification}");
            }
            var type = GetIdentifierType(externalId: identifierType);
            var value = type.Parse(identifierSpecification.Value);
            return value;
        }

        private IdentifierType GetIdentifierType(string externalId)
        {
            var identifierType = identifierTypes.GetIdentifierType(externalId);
            if (identifierType == null) throw new InvalidOperationException($"Unknown identifier type {externalId}");
            return identifierType;
        }
    }
}