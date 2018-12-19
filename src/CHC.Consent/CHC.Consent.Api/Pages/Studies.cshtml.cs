using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Api.Infrastructure.IdentifierDisplay;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace CHC.Consent.Api.Pages
{
    public class StudiesModel : PageModel
    {
        private ILogger<StudiesModel> Logger { get; }
        private readonly IUserProvider user;
        private readonly IPersonIdentifierDisplayHandlerProvider displayHandlerProvider;
        private readonly IConsentRepository consent;
        private readonly IIdentityRepository identifiers;
        private readonly IOptions<IdentifierDisplayOptions> displayOptions;

        public Dictionary<StudySubject, IDictionary<string, IEnumerable<PersonIdentifier>>> People { get; private set; }
        public IEnumerable<string> IdentifierNames { get; private set; }
        public Dictionary<string, string> DisplayNames { get; private set; }

        public Study Study { get; private set; }

        /// <inheritdoc />
        public StudiesModel(
            IConsentRepository consent,
            IIdentityRepository identifiers,
            IUserProvider user, 
            IPersonIdentifierDisplayHandlerProvider displayHandlerProvider,
            IOptions<IdentifierDisplayOptions> displayOptions,
            ILogger<StudiesModel> logger
            )
        {
            Logger = logger;
            this.user = user;
            this.displayHandlerProvider = displayHandlerProvider;
            this.consent = consent;
            this.identifiers = identifiers;
            this.displayOptions = displayOptions;
        }

        public ActionResult OnGet(long id)
        {
            Study = consent.GetStudies(user).SingleOrDefault(_ => _.Id == id);
            if (Study == null) return NotFound();

            IdentifierNames = displayOptions.Value.Default;
            DisplayNames = IdentifierNames.ToDictionary(
                name => name,
                name => displayHandlerProvider.GetDisplayHandler(name).DisplayName);

            var studyIdentity = Study.Id;
            var consentedSubjects = consent.GetConsentedSubjects(studyIdentity);
            Logger.LogDebug(
                "Found {count} consentedPeople - {consentedPeopleIds}",
                consentedSubjects.Count(),
                consentedSubjects);

            var peopleDetails = identifiers.GetPeopleWithIdentifiers(consentedSubjects.Select(_ => _.PersonId), IdentifierNames, user);

            People =
                (from s in consentedSubjects
                    join p in peopleDetails on s.PersonId equals p.Key
                    select new {s, p}
                ).ToDictionary(o => o.s, o => o.p.Value);
                    
            return Page();
        }       
    }
}