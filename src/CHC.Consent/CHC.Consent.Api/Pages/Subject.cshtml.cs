using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.EFCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CHC.Consent.Api.Pages
{
    public class Subject : PageModel
    {
        private readonly IUserProvider user;
        private readonly IConsentRepository consent;
        private readonly IIdentityRepository identity;

        /// <inheritdoc />
        public Subject(IUserProvider user, IConsentRepository consent, IIdentityRepository identity)
        {
            this.user = user;
            this.consent = consent;
            this.identity = identity;
        }

        public IActionResult OnGet(long studyId, string subjectIdentifier)
        {
            var studySubject = consent.FindStudySubject(new StudyIdentity(studyId), subjectIdentifier);
            if (studySubject == null) return NotFound();
            ActiveConsents = consent.GetActiveConsentsForSubject(new StudyIdentity(studyId), subjectIdentifier, user);
            Identifiers = identity.GetPersonIdentifiers(studySubject.PersonId);
            
            return Page();
        }


        public IEnumerable<Common.Consent.Consent> ActiveConsents { get; private set; }
        public IEnumerable<PersonIdentifier> Identifiers { get; set; }

        public PersonIdentifier GetIdentifier(string systemName)
        {
            return Identifiers.FirstOrDefault(_ => _.Definition.SystemName == systemName);
        }
    }
}