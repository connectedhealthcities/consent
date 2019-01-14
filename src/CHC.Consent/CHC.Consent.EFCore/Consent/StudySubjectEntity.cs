using CHC.Consent.Common.Consent;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore.Consent
{
    public class StudySubjectEntity : IEntity 
    {
        public long Id { get; set; }

        public PersonEntity Person { get; set; }

        public StudyEntity Study { get; set; }
        
        public string SubjectIdentifier { get; set; }
    }
}