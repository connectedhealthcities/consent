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
            IEnumerable<IEnumerable<PersonIdentifier>> matchSpecification) =>
            matchSpecification.Select(repository.FindPersonBy).FirstOrDefault(person => person != null);
    }
}