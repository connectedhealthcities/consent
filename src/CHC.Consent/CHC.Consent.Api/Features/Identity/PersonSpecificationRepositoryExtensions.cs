using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;

namespace CHC.Consent.Api.Features.Identity
{
    public static class PersonSpecificationRepositoryExtensions
    {
        public static Person FindPerson(this IdentityRepository repository, IEnumerable<IEnumerable<IIdentifier>> matchSpecification)
        {
            foreach (var specification in matchSpecification)
            {
                var person = repository.FindPersonBy(specification);
                if (person != null) 
                    return person;
            }

            return null;
        }

        public static Person CreatePerson(this IdentityRepository repository, IEnumerable<IIdentifier> identifiers)
        {
            return repository.AddPerson(UpdatePerson(new Person(), identifiers));
        }

        public static Person UpdatePerson(this Person person, IEnumerable<IIdentifier> identifiers)
        {
            foreach (var identifier in identifiers)
            {
                identifier.Update(person);
            }

            return person;
        }
    }
}