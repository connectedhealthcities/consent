using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using CHC.Consent.Api.Client;
using CHC.Consent.Api.Client.Models;
using Serilog;
using Serilog.Context;

namespace CHC.Consent.DataTool.Features.ImportData
{
    internal class XmlImporter
    {
        private ApiClientProvider ApiClientProvider { get; }

        /// <inheritdoc />
        public XmlImporter(ILogger log, ApiClientProvider apiClientProvider)
        {
            ApiClientProvider = apiClientProvider;
            
            Log = log.ForContext<XmlImporter>();
        }

        private ILogger Log { get; }

        public async Task Import(string source)
        {
            var client = await ApiClientProvider.CreateApiClient();
            var identifierDefinitions = client.GetIdentityStoreMetadata();
            Log.Debug("Available Identifiers Are {@identifiers}", identifierDefinitions);
            var evidenceDefinitions = client.GetConsentStoreMetadata();
            Log.Debug("Available Evidences Are {@evidences}", evidenceDefinitions);

            var xmlParser = new XmlParser(identifierDefinitions, evidenceDefinitions);
            using (LogContext.PushProperty("PeopleSource", source))
            using (var xmlReader = XmlReader.Create(source))
            {
                foreach (var person in xmlParser.GetPeople(xmlReader))
                {
                    Log.Verbose("Processing {@person}", person);
                    var api = await ApiClientProvider.CreateApiClient();

                    var personId = api.PutPerson(person.PersonSpecification);
                    //TODO: handle null personId - why would this happen?

                    Log.Debug("Person Id is {@personId}", personId);

                    using (LogContext.PushProperty("PersonId", personId.PersonId))
                    {
                        if (!person.ConsentSpecifications.Any())
                        {
                            Log.Debug("No consents provided");
                            continue;
                        }

                        foreach (var consent in person.ConsentSpecifications)
                        {
                            using (LogContext.PushProperty("StudyId", consent.StudyId))
                            {
                                RecordConsent(api, consent, personId.PersonId);
                            }
                        }
                    }
                }
            }
        }

        private void RecordConsent(IApi api, ImportedConsentSpecification consent, long personId)
        {
            Log.Debug("Processing Consent GivenOn {date} for person {personId}", consent.DateGiven, personId);
            Log.Verbose("Processing consent {@consent}", consent);

            SearchResult givenBy = null;
            if (consent.GivenBy != null && consent.GivenBy.Length > 0)
            {
                Log.Verbose("Find consent provider from {@givenBy}", consent.GivenBy);
                givenBy = api.FindPerson(consent.GivenBy);

                if (givenBy == null)
                {
                    Log.Verbose(
                        "Cannot find person who gave consent - {@specification}",
                        new object[] {consent.GivenBy});
                    Log.Error("Cannot find person who gave consent");
                    throw new NotImplementedException("Cannot find ");
                }
            }

            var existingSubject = api.FindBySubjectId(consent.StudyId, personId);

            string subjectIdentifier;
            if (existingSubject == null)
            {
                Log.Debug(
                    "No SubjectId for Person {@person} in Study {study} - requesting a new one",
                    personId,
                    consent.StudyId);
                subjectIdentifier = api.Generate(consent.StudyId).Value;
            }
            else
            {
                subjectIdentifier = existingSubject.SubjectIdentifier;
            }

            Log.Debug(
                "Sending consent for Person {@person} in Study {studyId} with SubjectIdentifier {subjectIdentifier}",
                personId,
                consent.StudyId,
                subjectIdentifier);
            api.PutConsent(
                new ConsentSpecification(
                    consent.StudyId,
                    subjectIdentifier,
                    personId,
                    consent.DateGiven,
                    consent.Evidence,
                    givenBy?.PersonId));
        }
    }
}