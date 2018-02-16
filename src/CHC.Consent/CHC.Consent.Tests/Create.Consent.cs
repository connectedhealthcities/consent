using System;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Consent.Evidences;
using FakeItEasy;

namespace CHC.Consent.Tests
{
    public static partial class Create
    {
        public static ConsentBuilder Consent => new ConsentBuilder();
        public static StudyBuilder Study => new StudyBuilder();
        public static StudySubjectBuilder StudySubject => new StudySubjectBuilder();

        public class StudySubjectBuilder : Builder<StudySubject, StudySubjectBuilder>
        {
            private Builder<Study> study = Create.Study;
            private string subjectIdentifier = Guid.NewGuid().ToString();
            private long personId = new Random().Next();


            public StudySubjectBuilder WithStudy(Builder<Study> newStudy) => Copy(_ => _.study = newStudy);

            public StudySubjectBuilder WithSubjectIdentifier(string newSubjectIdentifier)
                => Copy(_ => _.subjectIdentifier = newSubjectIdentifier);

            public StudySubjectBuilder WithSubjectPersonId(long newPersonId) => Copy(_ => _.personId = newPersonId);
            
            /// <inheritdoc />
            public override StudySubject Build()
            {
                return new StudySubject(study, subjectIdentifier, personId);
            }
        }

        public class ConsentBuilder : Builder<Common.Consent.Consent, ConsentBuilder>
        {
            private Builder<StudySubject> studySubject = Create.StudySubject;
            
            private Identifier[] identifiers = Array.Empty<Identifier>();
            private string pregnancyNumber = null;

            private DateTime dateGiven = 5.April(1914);
            private Evidence evidence = new MedwayEvidence {ConsentTakenBy = "Gary Leeming"};

            private DateTime? withdrawnDate = null;
            private Evidence withdrawnEvidence = null;

            public ConsentBuilder GivenOn(DateTime when) => Copy(_ => _.dateGiven = when);
            public ConsentBuilder WithGivenEvidence(Evidence newEvidence) => Copy(_ => _.evidence = newEvidence);
            
            public ConsentBuilder WithPregnancyNumber(string newPregnancyNumber) =>
                Copy(_ => _.pregnancyNumber = newPregnancyNumber);

            public ConsentBuilder WithStudySubject(Builder<StudySubject> newStudySubject)
                => Copy(_ => _.studySubject = newStudySubject);

            public ConsentBuilder Withdrawn(DateTime? newWithdrawnDate = null, Evidence newWithdrawnEvidence = null) => Copy(
                _ =>
                {
                    _.withdrawnDate = newWithdrawnDate ?? dateGiven.AddMonths(1);
                    _.withdrawnEvidence = newWithdrawnEvidence ?? A.Dummy<Evidence>();
                });

            /// <inheritdoc />
            public override Common.Consent.Consent Build()
            {
                return new Common.Consent.Consent(studySubject, dateGiven, evidence, identifiers)
                {
                    PregnancyNumber = pregnancyNumber,
                    Withdrawn = withdrawnDate,
                    WithdrawnEvidence = withdrawnEvidence
                };
            }
        }

        public class StudyBuilder : Builder<Study, StudyBuilder>
        {
            private string studyId = Guid.NewGuid().ToString();

            public StudyBuilder WithId(string newId) => Copy(_ => _.studyId = newId);

            /// <inheritdoc />
            public override Study Build()
            {
                return new Study(studyId);
            }
        }
    }
}