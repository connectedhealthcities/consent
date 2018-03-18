using System;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Consent;
using CHC.Consent.EFCore.Entities;
using Remotion.Linq.Parsing;

namespace CHC.Consent.EFCore
{
    public class SubjectIdentifierRepository : ISubjectIdentifierRepository
    {
        private IStore<StudyEntity> Studies { get; }
        private IStore<SubjectIdentifierEntity> SubjectIdentifiers { get; }

        /// <inheritdoc />
        public SubjectIdentifierRepository(
            IStore<StudyEntity> studies, 
            IStore<SubjectIdentifierEntity> subjectIdentifiers)
        {
            Studies = studies;
            SubjectIdentifiers = subjectIdentifiers;
        }

        /// <inheritdoc />
        public string GenerateIdentifier(StudyIdentity studyIdentity)
        {
            var current = SubjectIdentifiers.SingleOrDefault(_ => _.StudyId == studyIdentity.Id);

            if (current == null)
            {
                if (!Studies.Any(_ => _.Id == studyIdentity.Id))
                {
                    throw new InvalidOperationException($"No study found for {studyIdentity}");
                }

                current = SubjectIdentifiers.Add(new SubjectIdentifierEntity {StudyId = studyIdentity});
            }

            var id = current.CurrentValue += 1;
            return $"{id:x16}";
        }
    }
}