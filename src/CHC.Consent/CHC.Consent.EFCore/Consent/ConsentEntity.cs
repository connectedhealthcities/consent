using System;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore.Consent
{
    public class ConsentEntity : IEntity
    {
        /// <inheritdoc />
        public long Id { get; protected set; }
        public PersonEntity GivenBy { get; set; }
        public StudySubjectEntity StudySubject { get; set; }
        public DateTime DateProvided { get; set; }
        public DateTime? DateWithdrawn { get; set; }
    }
}