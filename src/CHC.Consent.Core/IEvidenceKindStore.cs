using CHC.Consent.Common.Core;

namespace CHC.Consent.Core
{
    public interface IEvidenceKindStore
    {
        IEvidenceKind FindEvidenceKindByExternalId(string recordEvidenceKindExternalId);
    }
}