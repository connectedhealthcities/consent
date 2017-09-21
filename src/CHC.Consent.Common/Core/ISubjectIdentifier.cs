namespace CHC.Consent.Common.Core
{
    public interface ISubjectIdentifier
    {
        long StudyId { get; }
        string Identifier { get; }
    }
}