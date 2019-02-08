using System.Linq;
using CHC.Consent.EFCore.Consent;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore.Identity
{
    public class HasConsentForStudyCriteria : ICriteria<PersonEntity>
    {
        private readonly long studyId;

        /// <inheritdoc />
        public HasConsentForStudyCriteria(long studyId)
        {
            this.studyId = studyId;
        }

        /// <inheritdoc />
        public IQueryable<PersonEntity> ApplyTo(IQueryable<PersonEntity> queryable, ConsentContext context)
        {
            return
                from person in queryable
                join consent in context.Set<ConsentEntity>()
                    on person.Id equals consent.StudySubject.Person.Id
                where consent.StudySubject.Study.Id == studyId
                select person;
        }
    }
}