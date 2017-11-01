using System;
using System.Linq;
using CHC.Consent.Common.Core;
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
            return GetSubjectForRead(studyId, subjectIdentifier);
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

        /// <inheritdoc />
        public IConsent AddConsent(Guid studyId, string subjectIdentifier, DateTimeOffset whenGiven, string[] evidence)
        {
            var subject = GetSubjectForWrite(studyId, subjectIdentifier);

            var session = getSession();

            if (session.Query<Consent.Consent>().Any(_ => _.DateWithdrawlRecorded == null && _.Subject == subject))
            {
                throw new NotImplementedException(
                    "We don't handle the case of having multiple active consents, or updating an existing consent");
            }

            var consent = new Consent.Consent(subject, whenGiven, evidence.Select(_ => new Evidence{TheEvidence = _}))
            {
                Authenticatable = security.GetCurrentAuthenticatable(),
                Date = Clock.CurrentDateTimeOffset()
            };

            session.Save(consent);

            return consent;
        }

        private Subject GetSubjectForWrite(Guid studyId, string subjectIdentifier)
        {
            var subject = GetSubjectForRead(studyId, subjectIdentifier);
            if (!security.CanWriteTo(subject))
            {
                throw new AccessDeniedException(
                    $"User#{security.UserAccessor.GetUser()} cannot add consent to {subject}");
            }
            return subject;
        }

        private Subject GetSubjectForRead(Guid studyId, string subjectIdentifier)
        {
            var subject = security.Readable(db => db.Query<Subject>().Where(_ => _.Study.Id == studyId))
                .SingleOrDefault(_ => _.Identifier == subjectIdentifier);

            if (subject == null)
            {
                throw new SubjectNotFoundException(studyId, subjectIdentifier);
            }
            return subject;
        }
    }
}