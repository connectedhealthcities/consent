using System;
using System.Collections.Generic;
using CHC.Consent.Common.Infrastructure.Data;

namespace CHC.Consent.Common.Consent
{
    public class Consent : IEntity
    {
        public long Id { get; set; }

        public StudySubject StudySubject { get; }

        public DateTime DateGiven { get; }

        public Evidence GivenEvidence { get;  }

        /// <inheritdoc />
        public Consent(StudySubject studySubject, DateTime dateGiven, Evidence givenEvidence, IEnumerable<Identifier> identifiers)
        {
            StudySubject = studySubject;
            DateGiven = dateGiven;
            GivenEvidence = givenEvidence;
            foreach (var identifier in identifiers ?? Array.Empty<Identifier>())
            {
                identifier.Update(this);
            }
        }

        public string PregnancyNumber { get; set; }

        public DateTime? Withdrawn { get; set; }

        public Evidence WithdrawnEvidence { get; set; }
    }
}