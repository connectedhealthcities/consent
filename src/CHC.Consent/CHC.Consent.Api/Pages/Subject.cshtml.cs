using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Consent.Evidences;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.EFCore;
using IdentityModel;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CHC.Consent.Api.Pages
{
    public class Subject : PageModel
    {
        private readonly IConsentRepository consents;
        private readonly IIdentityRepository identity;
        private readonly IStudySubjectRepository subjects;
        private readonly IUserProvider user;

        /// <inheritdoc />
        public Subject(IUserProvider user, ConsentContext db, IStudySubjectRepository subjects,
            IConsentRepository consents, IIdentityRepository identity)
        {
            Db = db;
            this.user = user;
            this.subjects = subjects;
            this.consents = consents;
            this.identity = identity;
        }

        private ConsentContext Db { get; }

        [BindProperty(SupportsGet = true)] public string ReturnUrl { get; set; }


        private StudySubject StudySubject { get; set; }
        public Common.Consent.Consent CurrentConsent { get; private set; }
        private IEnumerable<Common.Consent.Consent> Consents { get; set; }
        public bool ConsentIsActive { get; set; }
        private IEnumerable<PersonIdentifier> Identifiers { get; set; }

        [BindProperty(SupportsGet = false)] public InputModel Input { get; set; }

        public IActionResult OnGet(long studyId, string subjectIdentifier)
        {
            InitialiseBindingData(studyId, subjectIdentifier);

            if (StudySubject == null) return NotFound();
            return Page();
        }

        private void InitialiseBindingData(long studyId, string subjectIdentifier)
        {
            StudySubject = subjects.GetStudySubject(new StudyIdentity(studyId), subjectIdentifier);
            if (StudySubject == null) return;
            Consents = consents.GetConsentsForSubject(new StudyIdentity(studyId), subjectIdentifier);
            CurrentConsent = Consents.FirstOrDefault();
            ConsentIsActive = CurrentConsent != null && CurrentConsent.DateWithdrawn == null;
            Identifiers = identity.GetPersonIdentifiers(StudySubject.PersonId);
        }

        public IActionResult OnPost(long studyId, string subjectIdentifier)
        {
            InitialiseBindingData(studyId, subjectIdentifier);
            if (StudySubject == null) return NotFound();

            consents.WithdrawConsent(
                StudySubject,
                KnownEvidence.Website.Create(
                    KnownEvidence.WebsiteParts.User.Create(
                        KnownEvidence.WebsiteParts.UserParts.Name.Create(User.GetDisplayName()),
                        KnownEvidence.WebsiteParts.UserParts.Id.Create(User.GetSubjectId()),
                        KnownEvidence.WebsiteParts.UserParts.Email.Create(
                            User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(JwtClaimTypes.Email))
                    )
                ),
                KnownEvidence.Bib4AllWithdrawal.Create(
                    KnownEvidence.Bib4AllWithdrawalParts.RequestedBy.Create(Input.WithdrawnByName),
                    KnownEvidence.Bib4AllWithdrawalParts.Relationship.Create(Input.WithdrawnByRelationship)
                )
            );

            Db.SaveChanges();

            return RedirectToPage(new {ReturnUrl});
        }

        public PersonIdentifier GetIdentifier(string systemName)
        {
            return Identifiers.FirstOrDefault(_ => _.Definition.SystemName == systemName);
        }

        public class InputModel
        {
            [Required(AllowEmptyStrings = false), DisplayName("Full Name")]
            public string WithdrawnByName { get; set; }

            [Required(AllowEmptyStrings = false), DisplayName("Relationship")]
            public string WithdrawnByRelationship { get; set; }
        }
    }
}