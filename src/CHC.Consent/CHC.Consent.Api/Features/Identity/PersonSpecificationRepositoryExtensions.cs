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
            return matchSpecification
                .Select(repository.FindPersonBy)
                .FirstOrDefault(person => person != null);
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