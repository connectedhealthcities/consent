using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.EFCore.Consent;

namespace CHC.Consent.EFCore
{
    public class StudyRepository : IStudyRepository
    {
        public IStore<StudyEntity> Studies { get; }

        public StudyRepository(IStore<StudyEntity> studies)
        {
            Studies = studies;
        }

        public IEnumerable<Study> GetStudies(IUserProvider user) =>
            Studies.Select(_ => new Study(_.Id, _.Name))
                .ToArray();

        Study IStudyRepository.GetStudy(long studyId) =>
            Studies.Select(_ => new Study(_.Id, _.Name))
                .SingleOrDefault(_ => _.Id == studyId);

        public StudyEntity GetStudy(long studyId) => Studies.SingleOrDefault(_ => _.Id == studyId);
    }
}