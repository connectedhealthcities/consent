using System;

namespace CHC.Consent.WebApi.Abstractions
{
    public class SubjectNotFoundException : NotFoundException
    {
        public SubjectNotFoundException(Guid studyId, string subjectIdentifier) : base($"Cannot find subject#{subjectIdentifier} for Study#{studyId}")
        {
            
        }
    }
}