using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;

namespace CHC.Consent.Api.Features.Identity
{
    public static class PersonSpecificationRepositoryExtensions
    {
        public static PersonIdentity FindPerson(
            this IIdentityRepository repository,
            IEnumerable<IEnumerable<PersonIdentifier>> matchSpecification)
        {
            PersonIdentity FindPerson(CompositePersonSpecification specification)
                => repository.FindPersonBy(specification);

            return matchSpecification
                .Select(ConvertToSpecification)
                .Select(FindPerson)
                .FirstOrDefault(personIdentity => personIdentity != null);
        }

        private static CompositePersonSpecification ConvertToSpecification(IEnumerable<PersonIdentifier> identifiers)
        {
            return new CompositePersonSpecification(identifiers.Select(_ => (PersonIdentifierSpecification) _));
        }
    }
}