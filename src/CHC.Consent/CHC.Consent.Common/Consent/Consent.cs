using System;
using System.Collections.Generic;
using System.Linq;

namespace CHC.Consent.Common.Consent
{
    public class ConsentIdentity : IdentityBase
    {
        /// <inheritdoc />
        public ConsentIdentity(long id) : base(id)
        {
        }
    } 
    
    public class Consent  
    {
        public StudySubject StudySubject { get; }
        
        public long GivenByPersonId { get; }

        public DateTime DateGiven { get; }

        public IEnumerable<Evidence> GivenEvidence { get;  }
        public IEnumerable<CaseIdentifier> CaseIdentifiers { get; }

        /// <inheritdoc />
        public Consent(StudySubject studySubject, DateTime dateGiven, long givenByPersonId, IEnumerable<Evidence> givenEvidence, IEnumerable<CaseIdentifier> identifiers)
        {
            StudySubject = studySubject;
            GivenByPersonId = givenByPersonId;
            DateGiven = dateGiven;
            GivenEvidence = givenEvidence == null ? Array.Empty<Evidence>() : givenEvidence.ToArray();
            CaseIdentifiers = identifiers == null ? Array.Empty<CaseIdentifier>() : identifiers.ToArray();
        }
    }
}