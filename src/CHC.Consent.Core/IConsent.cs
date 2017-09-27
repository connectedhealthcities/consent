namespace CHC.Consent.Common.Core
{
    public interface IConsent
    {
        IStudy Study { get; }
        string SubjectIdentifier { get; }
        
        IEvidence ProvidedEvidence { get; }
        IEvidence WithdrawnEvidence { get; }
    }
}