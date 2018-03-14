using System;
using System.Collections.Generic;
using CHC.Consent.Common.Infrastructure.Data;

namespace CHC.Consent.Common.Consent
{
    public class ConsentIdentity : IdentityBase
    {
        /// <inheritdoc />
        public ConsentIdentity(long id) : base(id)
        {
        }
    } 
    
    public class Consent : IEntity
    {
        public long Id { get; set; }

        public StudySubject StudySubject { get; }
        
        public long GivenByPersonId { get; }

        public DateTime DateGiven { get; }

        public Evidence GivenEvidence { get;  }

        /// <inheritdoc />
        public Consent(StudySubject studySubject, DateTime dateGiven, long givenByPersonId, Evidence givenEvidence, IEnumerable<ConsentIdentifier> identifiers)
        {
            StudySubject = studySubject;
            GivenByPersonId = givenByPersonId;
            DateGiven = dateGiven;
            GivenEvidence = givenEvidence;
            foreach (var identifier in identifiers ?? Array.Empty<ConsentIdentifier>())
            {
                identifier.Update(this);
            }
        }

        public string PregnancyNumber { get; set; }

        public DateTime? Withdrawn { get; set; }

        public Evidence WithdrawnEvidence { get; set; }
    }
}