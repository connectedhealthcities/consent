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
        private IStudyRepository Studies { get; }
        private IStudySubjectRepository Subjects { get; }
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

        [BindProperty(SupportsGet = true)]
        public IList<SearchFieldGroup> SearchGroups { get; set; } = new List<SearchFieldGroup>();

        [BindProperty(SupportsGet = true)]
        public string SubjectIdentifier { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool IncludeWithdrawnParticipants { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public bool Search { get; set; }

        public class SearchFieldGroup
        {
            public IList<SearchField> Fields { get; set; } = new List<SearchField>();
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
            IStudyRepository studies,
            IStudySubjectRepository subjects,
            IConsentRepository consent,
            IIdentityRepository identityRepository,
            IdentifierDefinitionRegistry identifierDefinitionRegistry,
            IUserProvider user, 
            IOptions<IdentifierDisplayOptions> displayOptionsProvider,
            ILogger<StudiesModel> logger
            )
        {
            Studies = studies;
            Subjects = subjects;
            Logger = logger;
            this.user = user;
            this.consent = consent;
            this.identityRepository = identityRepository;
            this.identifierDefinitionRegistry = identifierDefinitionRegistry;
            displayOptions = displayOptionsProvider.Value;
            
            SearchGroups = GetDefinedSearchGroups();
        }

        public ActionResult OnGet([FromRoute]long id)
        {
            Study = Studies.GetStudy(id);
            if (Study == null) return NotFound();


            var studyIdentity = Study.Id;
            var consentedSubjects = Subjects.GetConsentedSubjects(studyIdentity);
            Logger.LogDebug(
                "Found {count} consentedPeople - {consentedPeopleIds}",
                consentedSubjects.Length,
                consentedSubjects);

            if (!Search)
            {
                SearchGroups = GetDefinedSearchGroups();
                return Page();
            }

            var mergedSearchGroups = GetSearchGroupsAndValues();

            SearchGroups = mergedSearchGroups;

            return DoSearch();
        }

        private SearchFieldGroup[] GetSearchGroupsAndValues()
        {
            var mergedSearchGroups = GetDefinedSearchGroups();
            for (var searchGroupIndex = 0;
                searchGroupIndex < Math.Min(SearchGroups.Count, mergedSearchGroups.Length);
                searchGroupIndex++)
            {
                var searchGroup = SearchGroups[searchGroupIndex];
                for (var fieldIndex = 0;
                    fieldIndex < Math.Min(searchGroup.Fields.Count, mergedSearchGroups[searchGroupIndex].Fields.Count);
                    fieldIndex++)
                {
                    mergedSearchGroups[searchGroupIndex].Fields[fieldIndex].Value =
                        searchGroup.Fields[fieldIndex].Value;
                }
            }

            return mergedSearchGroups;
        }

        private SearchFieldGroup[] GetDefinedSearchGroups()
        {
            return displayOptions.Search
                .Select(s => new SearchFieldGroup
                {
                    Fields = s.Fields.Select(field =>
                        new SearchField
                        {
                            FieldName = field.Name, Label = field.Label, Compare = field.Compare, DataType =
                                GetIdentifierDefinitionByPath(field.Name).Type.SystemName

                        }).ToArray()
                }).ToArray();
            
        }

        private ActionResult DoSearch()
        {
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
            var consentedSubjects =
                IncludeWithdrawnParticipants
                    ? Subjects.GetSubjectsWithLastWithdrawalDate(studyIdentity).Select(_ => _.studySubject)
                    : Subjects.GetConsentedSubjects(studyIdentity);
            Logger.LogDebug(
                "Found {count} consentedPeople - {consentedPeopleIds}",
                consentedSubjects.LongCount(),
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