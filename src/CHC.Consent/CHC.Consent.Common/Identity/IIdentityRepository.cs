using System.Collections.Generic;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;

namespace CHC.Consent.Common.Identity
{
    public interface IIdentityRepository
    {
        PersonIdentity FindPersonBy(IEnumerable<PersonIdentifier> identifiers);
        IEnumerable<PersonIdentifier> GetPersonIdentifiers(long personId);
        PersonIdentity CreatePerson(IEnumerable<PersonIdentifier> identifiers, Authority authority);
        void UpdatePerson(PersonIdentity personIdentity, IEnumerable<PersonIdentifier> identifiers, Authority authority);

        IDictionary<PersonIdentity, IEnumerable<PersonIdentifier>> GetPeopleWithIdentifiers(
            IEnumerable<PersonIdentity> personIds,
            IEnumerable<string> identifierNames,
            IUserProvider user);

        IDictionary<PersonIdentity, IEnumerable<PersonIdentifier>>
            GetPeopleWithIdentifiers(
                IEnumerable<PersonIdentity> personIds,
                IEnumerable<string> identifierNames,
                IUserProvider user,
                IEnumerable<IdentifierSearch> search
            );

        Authority GetAuthority(string systemName);
        Agency GetAgency(string systemName);
        string GetPersonAgencyId(PersonIdentity personId, AgencyIdentity agencyId);
    }
}