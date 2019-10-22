using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Consent.Evidences;
using CHC.Consent.EFCore.Consent;
using CHC.Consent.EFCore.Entities;
using Microsoft.EntityFrameworkCore;


namespace CHC.Consent.EFCore
{
    public class ConsentRepository : IConsentRepository
    {
        private StudySubjectRepository StudySubjects { get; }
        private IStore<PersonEntity> People { get; }
        private IStore<ConsentEntity> Consents { get; }
        private IStore<EvidenceEntity> Evidence { get; }
        private IdentifierXmlMarshallers<Evidence, EvidenceDefinition> EvidenceRegistry { get; }

        public ConsentRepository(
            StudySubjectRepository studySubjects, 
            IStore<PersonEntity> people,
            IStore<ConsentEntity> consents,
            IStore<EvidenceEntity> evidence,
            EvidenceDefinitionRegistry evidenceRegistry)
        {
            StudySubjects = studySubjects;
            People = people;
            Consents = consents;
            Evidence = evidence;
            EvidenceRegistry = new IdentifierXmlMarshallers<Evidence, EvidenceDefinition>(evidenceRegistry);
        }
           
        public ConsentIdentity FindActiveConsent(StudySubject studySubject)
        {
            var consents =
                Consents.Where(
                    _ => _.StudySubject.Person.Id == studySubject.PersonId.Id &&
                         _.StudySubject.Study.Id == studySubject.StudyId.Id &&
                         _.StudySubject.SubjectIdentifier == studySubject.SubjectIdentifier &&
                         _.DateWithdrawn == null);
                

            var consentId = consents.Select(_ => (long?)_.Id).SingleOrDefault();
            return consentId == null ? null : new ConsentIdentity(consentId.Value);
        }

        /// <inheritdoc />
        public ConsentIdentity AddConsent(Common.Consent.Consent consent)
        {
            var subject = StudySubjects.FindStudySubjectForPerson(consent.StudySubject.StudyId, consent.StudySubject.PersonId).FirstOrDefault();
            
            if(subject == null) throw new NotImplementedException($"StudySubject#{consent.StudySubject} not found");

            PersonEntity givenBy = null;
            if (consent.GivenByPersonId != null)
            {
                givenBy = People.Get(consent.GivenByPersonId.Value);
                if (givenBy == null) throw new NotImplementedException($"Person#{consent.GivenByPersonId} not found");
            }

            var saved = Consents.Add(
                new ConsentEntity {DateProvided = consent.DateGiven, GivenBy = givenBy, StudySubject = subject});

            foreach (var evidence in consent.GivenEvidence)
            {
                Evidence.Add(
                    new GivenEvidenceEntity
                    {
                        Value = EvidenceRegistry.MarshallToXml(evidence).ToString(SaveOptions.DisableFormatting),
                        Type = evidence.Definition.SystemName,
                        Consent = saved
                    });
            }

            return new ConsentIdentity(saved.Id);
        }

        
        private Expression<Func<EvidenceEntity, Evidence>> MarshallEvidenceFromEntity()
        {
            return evidence => EvidenceRegistry.MarshallFromXml(evidence.Type, XElement.Parse(evidence.Value));
        }
        

        

        /// <inheritdoc />
        public void WithdrawConsent(StudySubject studySubject, params Evidence[] allEvidence)
        {
            var activeConsent = ConsentsFor(studySubject.StudyId.Id, studySubject.SubjectIdentifier)
                .Where(_ => _.DateWithdrawn == null)
                .OrderByDescending(_ => _.DateProvided)
                .FirstOrDefault();
            
            activeConsent.DateWithdrawn = DateTime.Now;
            foreach (var evidence in allEvidence)
            {
                activeConsent.WithdrawnEvidence.Add(
                    new WithdrawnEvidenceEntity
                    {
                        Type = evidence.Definition.SystemName,
                        Value = EvidenceRegistry.MarshallToXml(evidence).ToString(SaveOptions.DisableFormatting),

                    }
                );
            }
        }

        private IQueryable<ConsentEntity> ConsentsFor(long studyId, string subjectIdentifier)
        {
            return Consents.Where(
                c => c.StudySubject.SubjectIdentifier == subjectIdentifier &&
                     c.StudySubject.Study.Id == studyId);
        }

        /// <inheritdoc />
        public IEnumerable<Common.Consent.Consent> GetConsentsForSubject(StudyIdentity studyIdentity, string subjectIdentifier)
        {
            return
                from c in (
                from c in ConsentsFor(studyIdentity, subjectIdentifier)
                    .Include(_ => _.GivenEvidence).Include(_ => _.WithdrawnEvidence)
                orderby c.DateProvided descending
                let subject = c.StudySubject
                let person = subject.Person
                select new
                {
                    StudySubject = new StudySubject(studyIdentity, subjectIdentifier, 
                        new PersonIdentity(person.Id)), 
                    c.DateProvided, 
                    c.DateWithdrawn,
                    GivenById = (long?)c.GivenBy.Id,
                    GivenEvidence = c.GivenEvidence.ToArray(),
                    WithdrawnEvidence = c.WithdrawnEvidence.ToArray()
                }
                ).ToArray()
                select new Common.Consent.Consent(
                    c.StudySubject,
                    c.DateProvided,
                    c.GivenById,
                    c.GivenEvidence?.AsQueryable().Select(MarshallEvidenceFromEntity()))
                {
                    DateWithdrawn = c.DateWithdrawn,
                    WithdrawnEvidence = c.WithdrawnEvidence?.AsQueryable().Select(MarshallEvidenceFromEntity())
                };

        }

        
    }
}