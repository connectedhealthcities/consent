using System.Collections.Generic;

namespace CHC.Consent.Common.Identity
{
    public interface IPersonIdentifierListChecker
    {
        void EnsureHasNoInvalidDuplicates(IEnumerable<IIdentifier> identifiers);
    }
}