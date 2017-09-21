using CHC.Consent.Common.Core;

namespace CHC.Consent.Common.SubjectIdentifierCreation
{
    public interface ISubjectIdentfierAllocator
    {
        string AllocateNewIdentifier(IStudy study);
    }
}