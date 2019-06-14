using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CHC.Consent.Api.Infrastructure.IdentifierDisplay;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.Common.Infrastructure.Definitions;
using CHC.Consent.Common.Infrastructure.Definitions.Types;
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
        private readonly IIdentityRepository identityRepository;
        private readonly IdentifierDefinitionRegistry identifierDefinitionRegistry;
        private readonly IdentifierDisplayOptions displayOptions;

        public IList<PersonDetails> People { get; private set; } = Array.Empty<PersonDetails>();
        public bool ShowPeople { get; private set; } = false;
        public IEnumerable<string> IdentifierNames { get; private set; }
        public Dictionary<string, string> IdentifierLabels { get; set; }

        [BindProperty]
        public IList<SearchFieldGroup> SearchGroups { get; set; } = new List<SearchFieldGroup>();

        [BindProperty]
        public string SubjectIdentifier { get; set; }

        public class SearchFieldGroup
        {
            public IList<SearchField> Fields { get; } = new List<SearchField>();
        }

        public class SearchField
        {
            public string FieldName { get; set; }
            public string Value { get; set; }
            public string Compare { get; set; }
            public string Label { get; set; }
            public string DataType { get; set; }
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
            IIdentityRepository identityRepository,
            IdentifierDefinitionRegistry identifierDefinitionRegistry,
            IUserProvider user, 
            IOptions<IdentifierDisplayOptions> displayOptionsProvider,
            ILogger<StudiesModel> logger
            )
        {
            Logger = logger;
            this.user = user;
            this.consent = consent;
            this.identityRepository = identityRepository;
            this.identifierDefinitionRegistry = identifierDefinitionRegistry;
            this.displayOptions = displayOptionsProvider.Value;
        }

        public ActionResult OnGet(long id)
        {
            Study = consent.GetStudies(user).SingleOrDefault(_ => _.Id == id);
            if (Study == null) return NotFound();

            foreach (var searchGroup in displayOptions.Search)
            {
                var inputGroup = new SearchFieldGroup();

                foreach (var field in searchGroup.Fields)
                {
                    var fieldType = GetIdentifierDefinitionByPath(field.Name).Type.SystemName;
                    inputGroup.Fields.Add(
                        new SearchField
                        {
                            FieldName = field.Name, Label = field.Label, Compare = field.Compare, DataType = fieldType
                        });
                }
                
                SearchGroups.Add(inputGroup);
            }

            var studyIdentity = Study.Id;
            var consentedSubjects = consent.GetConsentedSubjects(studyIdentity);
            Logger.LogDebug(
                "Found {count} consentedPeople - {consentedPeopleIds}",
                consentedSubjects.Count(),
                consentedSubjects);
                   
            return Page();
        }

        public ActionResult OnPost(long id)
        {
            Study = consent.GetStudies(user).SingleOrDefault(_ => _.Id == id);
            if (Study == null) return NotFound();

            IdentifierNames = displayOptions.Default;
            IdentifierLabels = IdentifierNames.ToDictionary(_ => _, path => GetIdentifierDefinitionByPath(path).Name);
            

            var identifierSearches = SearchGroups
                .SelectMany(_ => _.Fields)
                .Where(_ => !string.IsNullOrWhiteSpace(_.Value))
                .Select(_ => new IdentifierSearch {IdentifierName = _.FieldName, Value = _.Value, Operator = GetOperator(_)})
                .ToArray();

            if (string.IsNullOrEmpty(SubjectIdentifier) && !identifierSearches.Any())
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

            ShowPeople = true;
            var peopleDetails = identityRepository.GetPeopleWithIdentifiers(
                consentedSubjects.Select(_ => _.PersonId), 
                IdentifierNames, 
                user, 
                identifierSearches,
                SubjectIdentifier);

            People =
                (from p in peopleDetails
                    join s in consentedSubjects on p.Key equals s.PersonId
                    select new PersonDetails {Subject = s, Identifiers = p.Value}
                ).ToImmutableList();

            return Page();
        }

        private IdentifierDefinition GetIdentifierDefinitionByPath(string path)
        {
            var fieldNamePath = path.Split(IdentifierSearch.Separator);
            DefinitionRegistry registry = identifierDefinitionRegistry;
            foreach (var fieldName in fieldNamePath.Take(fieldNamePath.Length - 1))
            {
                var definition = registry[fieldName];
                var type = (CompositeDefinitionType) definition.Type;
                registry = type.Identifiers;
            }

            return (IdentifierDefinition) registry[fieldNamePath.Last()];
        }

        private static IdentifierSearchOperator GetOperator(SearchField searchField)
        {
            switch (searchField.Compare)
            {
                case "before":
                    return IdentifierSearchOperator.LessThanOrEqual;
                case "after":
                    return IdentifierSearchOperator.GreaterThanOrEqual;
                default:
                    return IdentifierSearchOperator.Contains;
            }
        }
    }
}