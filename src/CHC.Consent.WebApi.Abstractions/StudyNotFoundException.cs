using System;

namespace CHC.Consent.WebApi.Abstractions
{
    public class StudyNotFoundException : Exception
    {
        public Guid StudyId { get; }

        public StudyNotFoundException(Guid studyId) : base($"Study#{studyId} was not found")
        {
            StudyId = studyId;
            Data["StudyId"] = studyId;
        }
    }
}