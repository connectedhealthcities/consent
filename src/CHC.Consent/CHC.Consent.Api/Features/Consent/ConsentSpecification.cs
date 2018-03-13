using System;
using System.ComponentModel.DataAnnotations;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Identity;

namespace CHC.Consent.Api.Features.Consent
{
    public class ConsentSpecification
    {
        [Required]
        public long StudyId { get; set; }
        [Required]
        public string SubjectIdentifier { get; set; }
        [Required]
        public long PersonId { get; set; }
        
        [Required]
        public DateTime DateGiven { get; set; }
        
        [Required]
        public Evidence Evidence { get; set; }
        
        public ConsentIdentifier[] CaseId { get; set; }
        
        [Required]
        public long GivenBy { get; set; } 
    }
}