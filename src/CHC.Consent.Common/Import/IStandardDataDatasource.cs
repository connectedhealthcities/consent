using System.Collections.Generic;
using CHC.Consent.Common.Evidence;
using CHC.Consent.Common.Identity;

namespace CHC.Consent.Common.Import
{
    public interface IStandardDataDatasource
    {
        IEnumerable<IPerson> People { get; }
    }

    public interface IPerson
    {
        IDictionary<IdentityKind, Identity.Identity> Identities { get; }
        IDictionary<EvidenceKind, Evidence.Evidence> Evidence { get; }
    }
}