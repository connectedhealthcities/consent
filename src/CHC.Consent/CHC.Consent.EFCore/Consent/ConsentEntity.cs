using System;
using System.Collections.Generic;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.EFCore.Security;

namespace CHC.Consent.EFCore.Consent
{
    public class ConsentEntity : Securable, IEntity
    {
        public ConsentEntity() : base("Consent")
        {
        }

        /// <inheritdoc />
        public long Id { get; protected set; }
        public PersonEntity GivenBy { get; set; }
        public StudySubjectEntity StudySubject { get; set; }
        public DateTime DateProvided { get; set; }
        public DateTime? DateWithdrawn { get; set; }
        public ICollection<CaseIdentifierEntity> CaseIdentifiers { get; set; } = new List<CaseIdentifierEntity>();
        public ICollection<GivenEvidenceEntity> GivenEvidence { get; set; } = new List<GivenEvidenceEntity>();
        public ICollection<WithdrawnEvidenceEntity> WithdrawnEvidence { get; set; } = new List<WithdrawnEvidenceEntity>();
        
    }
}