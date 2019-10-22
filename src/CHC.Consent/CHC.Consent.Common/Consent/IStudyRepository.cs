using System.Collections.Generic;
using CHC.Consent.Common.Infrastructure;

namespace CHC.Consent.Common.Consent
{
    public interface IStudyRepository
    {
        IEnumerable<Study> GetStudies(IUserProvider user);
        Study GetStudy(long studyId);
    }
}