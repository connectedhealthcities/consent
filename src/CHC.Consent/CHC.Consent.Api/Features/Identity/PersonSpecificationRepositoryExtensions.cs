using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;

namespace CHC.Consent.Api.Features.Identity
{
    public static class PersonSpecificationRepositoryExtensions
    {
        public static Person FindPerson(this IdentityRepository repository, ParsedPersonSpecification specification)
        {
            return specification.Matches()
                .Select(repository.FindPersonBy)
                .FirstOrDefault(person => person != null);
        }

        public static Person CreatePerson(this IdentityRepository repository, ParsedPersonSpecification specification)
        {
            return repository.AddPerson(UpdatePerson(new Person(), specification));
        }

        public static Person UpdatePerson(this Person person, ParsedPersonSpecification specification)
        {
            foreach (var identifier in specification.Identifiers)
            {
                identifier.Update(person);
            }

            return person;
        }
    }
}