using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using CHC.Consent.Api.Infrastructure.IdentifierDisplay;
using CHC.Consent.Common;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.EFCore.Consent;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.EFCore.Security;
using IdentityModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace CHC.Consent.Api.Pages
{
    public class StudiesModel : PageModel
    {
        public ILogger<StudiesModel> Logger { get; }
        private readonly IUserProvider user;
        private readonly IPersonIdentifierDisplayHandlerProvider diplayHandlerProvider;
        private readonly IConsentRepository consent;
        private readonly IIdentityRepository identifiers;
        private readonly IOptions<IdentifierDisplayOptions> displayOptions;

        public IDictionary<PersonIdentity, IDictionary<string, IEnumerable<IPersonIdentifier>>> People { get; private set; }
        public IEnumerable<string> IdentifierNames { get; private set; }
        public Dictionary<string, string> DisplayNames { get; private set; }

        public Study Study { get; private set; }

        /// <inheritdoc />
        public StudiesModel(
            IConsentRepository consent,
            IIdentityRepository identifiers,
            IUserProvider user, 
            IPersonIdentifierDisplayHandlerProvider diplayHandlerProvider,
            IOptions<IdentifierDisplayOptions> displayOptions,
            ILogger<StudiesModel> logger
            )
        {
            Logger = logger;
            this.user = user;
            this.diplayHandlerProvider = diplayHandlerProvider;
            this.consent = consent;
            this.identifiers = identifiers;
            this.displayOptions = displayOptions;
        }

        public ActionResult OnGet(long id)
        {
            Study = consent.GetStudies(user).SingleOrDefault(_ => _.Id == id);
            if (Study == null) return NotFound();

            var studyIdentity = Study.Id;
            var consentedPeopleIds = consent.GetConsentedPeopleIds(studyIdentity);
            Logger.LogDebug(
                "Found {count} consentedPeople - {consentedPeopleIds}",
                consentedPeopleIds.Count(),
                consentedPeopleIds);

            IdentifierNames = displayOptions.Value.Default;
            People = identifiers.GetPeopleWithIdentifiers(consentedPeopleIds, IdentifierNames, user);

            DisplayNames = IdentifierNames.ToDictionary(
                name => name,
                name => diplayHandlerProvider.GetDisplayHandler(name).DisplayName); 
            
            return Page();
        }

       
    }
}