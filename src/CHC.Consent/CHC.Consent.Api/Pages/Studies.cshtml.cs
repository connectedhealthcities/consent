using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        private readonly IConsentRepository consent;
        private readonly IIdentityRepository identifiers;
        private readonly IdentifierDisplayOptions displayOptions;

        public IList<PersonDetails> People { get; private set; } = Array.Empty<PersonDetails>();
        public IEnumerable<string> IdentifierNames { get; private set; }
        
        [BindProperty]
        public IList<SearchField> SearchFields { get; set; } = new List<SearchField>();

        public class SearchField
        {
            public string FieldName { get; set; }
            public string Value { get; set; }
            public string Label { get; set; }
        }

        public class PersonDetails
        {
            public StudySubject Subject { get; set; }
            public IEnumerable<PersonIdentifier> Identifiers { get; set; }

            public void Deconstruct(out StudySubject subject, out IEnumerable<PersonIdentifier> identifiers)
            {
                subject = Subject;
                identifiers = Identifiers;
            }
        }

        public Study Study { get; private set; }

        /// <inheritdoc />
        public StudiesModel(
            IConsentRepository consent,
            IIdentityRepository identifiers,
            IUserProvider user, 
            IOptions<IdentifierDisplayOptions> displayOptionsProvider,
            ILogger<StudiesModel> logger
            )
        {
            Logger = logger;
            this.user = user;
            this.consent = consent;
            this.identifiers = identifiers;
            this.displayOptions = displayOptionsProvider.Value;
        }

        public ActionResult OnGet(long id)
        {
            Study = consent.GetStudies(user).SingleOrDefault(_ => _.Id == id);
            if (Study == null) return NotFound();

            IdentifierNames = displayOptions.Default;

            foreach (var field in displayOptions.Search)
            {
                SearchFields.Add(new SearchField{FieldName = field.Name, Label = field.Label});
            }
        
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
                    select new PersonDetails {Subject = s, Identifiers = p.Value}
                ).ToImmutableList();
                    
            return Page();
        }

        public ActionResult OnPost(long id)
        {
            Study = consent.GetStudies(user).SingleOrDefault(_ => _.Id == id);
            if (Study == null) return NotFound();

            IdentifierNames = displayOptions.Default;

            var searches = SearchFields.Where(_ => !string.IsNullOrWhiteSpace(_.Value)).Select(_ => new IdentifierSearch{ IdentifierName = _.FieldName, Value = _.Value})
                .ToArray();

            if (!searches.Any())
            {
                ModelState.AddModelError("", "Please enter a search criteria");
            }

            if (!ModelState.IsValid) return Page();


            var studyIdentity = Study.Id;
            var consentedSubjects = consent.GetConsentedSubjects(studyIdentity);
            Logger.LogDebug(
                "Found {count} consentedPeople - {consentedPeopleIds}",
                consentedSubjects.Count(),
                consentedSubjects);

            var peopleDetails = identifiers.GetPeopleWithIdentifiers(consentedSubjects.Select(_ => _.PersonId), IdentifierNames, user, searches);

            People =
                (from p in peopleDetails
                    join s in consentedSubjects on p.Key equals s.PersonId
                    select new PersonDetails {Subject = s, Identifiers = p.Value}
                ).ToImmutableList();

            return Page();
        }
    }
}