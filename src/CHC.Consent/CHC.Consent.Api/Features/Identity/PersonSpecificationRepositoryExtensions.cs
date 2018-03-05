using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;

namespace CHC.Consent.Api.Features.Identity
{
    public static class PersonSpecificationRepositoryExtensions
    {
        public static PersonIdentity FindPerson(this IIdentityRepository repository, IEnumerable<IEnumerable<IPersonIdentifier>> matchSpecification)
        {
            foreach (var specification in matchSpecification)
            {
                var person = repository.FindPersonBy(specification);
                if (person != null) 
                    return person;
            }

            return null;
        }
    }
}