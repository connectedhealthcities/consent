using System;

namespace CHC.Consent.Common.Consent
{
    public class Consent
    {
        public StudySubject StudySubject { get; }
        
        public DateTime DateGiven { get; }
        public Evidence GivenEvidence { get;  }

        /// <inheritdoc />
        public Consent(StudySubject studySubject, DateTime dateGiven, Evidence givenEvidence)
        {
            StudySubject = studySubject;
            DateGiven = dateGiven;
            GivenEvidence = givenEvidence;
        }

        public ushort PregnancyNumber { get; set; }
        
        public DateTime? Withdrawn { get; set; }
        public Evidence WithdrawnEvidence { get; set; }
    }
}