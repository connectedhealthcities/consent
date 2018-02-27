using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace CHC.Consent.Common.Identity
{
    public interface IIdentityRepository
    {
        PersonIdentity FindPersonBy(IEnumerable<IIdentifier> identifiers);
        IEnumerable<IIdentifier> GetPersonIdentities(long personId);
        PersonIdentity CreatePerson(IEnumerable<IIdentifier> identifiers);
        void UpdatePerson(PersonIdentity personIdentity, IEnumerable<IIdentifier> specificationIdentifiers);
    }
}