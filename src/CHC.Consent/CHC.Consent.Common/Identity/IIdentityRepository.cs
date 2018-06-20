using System.Collections.Generic;
using CHC.Consent.Common.Infrastructure;

namespace CHC.Consent.Common.Identity
{
    public interface IIdentityRepository
    {
        PersonIdentity FindPersonBy(IEnumerable<IPersonIdentifier> identifiers);
        IEnumerable<IPersonIdentifier> GetPersonIdentifiers(long personId);
        PersonIdentity CreatePerson(IEnumerable<IPersonIdentifier> identifiers);
        void UpdatePerson(PersonIdentity personIdentity, IEnumerable<IPersonIdentifier> specificationIdentifiers);

        IDictionary<PersonIdentity, IDictionary<string, IEnumerable<IPersonIdentifier>>> GetPeopleWithIdentifiers(
            IEnumerable<PersonIdentity> personIds,
            IEnumerable<string> identifierNames,
            IUserProvider user
        );
    }
}