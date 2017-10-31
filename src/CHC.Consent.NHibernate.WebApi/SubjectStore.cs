using System;
using System.Linq;
using CHC.Consent.Core;
using CHC.Consent.NHibernate.Consent;
using CHC.Consent.NHibernate.Security;
using CHC.Consent.Utils;
using CHC.Consent.WebApi.Abstractions;
using CHC.Consent.WebApi.Abstractions.Consent;
using NHibernate;
using NHibernate.Linq;

namespace CHC.Consent.NHibernate.WebApi
{
    public class SubjectStore : ISubjectStore
    {
        public IClock Clock { get; }
        private readonly SecurityHelper security;
        private readonly Func<ISession> getSession;

        /// <inheritdoc />
        public SubjectStore(SecurityHelper security, Func<ISession> getSession, IClock clock)
        {
            Clock = clock;
            this.security = security;
            this.getSession = getSession;
        }

        /// <inheritdoc />
        public IQueryable<ISubject> GetSubjects(Guid studyId)
        {
            return security.Readable(_ => _.Query<Subject>()).Where(subject => subject.Study.Id == studyId);
        }

        public ISubject GetSubject(Guid studyId, string subjectIdentifier)
        {
            return security.Readable(_ => _.Query<Subject>())
                .SingleOrDefault(subject => subject.Study.Id == studyId && subject.Identifier == subjectIdentifier);
        }

        /// <inheritdoc />
        public ISubject AddSubject(Guid studyId, string subjectIdentifier)
        {
            var session = getSession();

            var study = security.Readable(db => db.Query<Study>()).SingleOrDefault(_ => _.Id == studyId);

            if (study == null)
            {
                throw new StudyNotFoundException(studyId);
            }
            if (!security.CanWriteTo(study))
            {
                throw new AccessDeniedException($"User#{security.UserAccessor.GetUser()} cannot add subject to to {study}");
            }

            if (session.Query<Subject>().Any(_ => _.Study == study && _.Identifier == subjectIdentifier))
            {
                throw new SubjectAlreadyExistsException(study, subjectIdentifier);
            }

            var subject = new Subject(study, subjectIdentifier)
            {
                Authenticatable = security.GetCurrentAuthenticatable(),
                Date = Clock.CurrentDateTimeOffset()
            };
            
            session.Save(subject);
            
            return subject;
        }
    }
}