using System.Collections.Generic;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CHC.Consent.Api.Pages
{
    public class Subject : PageModel
    {
        private readonly IUserProvider user;
        private readonly IConsentRepository consent;

        /// <inheritdoc />
        public Subject(IUserProvider user, IConsentRepository consent)
        {
            this.user = user;
            this.consent = consent;
        }

        public void OnGet(long studyId, string subjectIdentifier)
        {
            ActiveConsents = consent.GetActiveConsentsForSubject(new StudyIdentity(studyId), subjectIdentifier, user);
        }

        public IEnumerable<Common.Consent.Consent> ActiveConsents { get; private set; }
    }
}