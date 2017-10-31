using System;
using System.Linq;
using CHC.Consent.Core;

namespace CHC.Consent.WebApi.Abstractions.Consent
{
    public interface ISubjectStore
    {
        IQueryable<ISubject> GetSubjects(Guid studyId);
        ISubject GetSubject(Guid studyId, string id);
        ISubject AddSubject(Guid studyId, string subjectIdentifier);
    }
}