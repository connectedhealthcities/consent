using CHC.Consent.Common.Consent;

namespace CHC.Consent.Common
{
    public interface ISubjectIdentifierRepository
    {
        string GenerateIdentifier(StudyIdentity studyIdentity);
    }
}