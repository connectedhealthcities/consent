using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace CHC.Consent.Common.Identity
{
    public interface IIdentityRepository
    {
        PersonIdentity FindPersonBy(IEnumerable<IPersonIdentifier> identifiers);
        IEnumerable<IPersonIdentifier> GetPersonIdentities(long personId);
        PersonIdentity CreatePerson(IEnumerable<IPersonIdentifier> identifiers);
        void UpdatePerson(PersonIdentity personIdentity, IEnumerable<IPersonIdentifier> specificationIdentifiers);
    }
}