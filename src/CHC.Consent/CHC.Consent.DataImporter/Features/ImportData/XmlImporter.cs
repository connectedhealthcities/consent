using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using CHC.Consent.Api.Client;
using CHC.Consent.Api.Client.Models;
using Microsoft.Extensions.Logging;

namespace CHC.Consent.DataImporter.Features.ImportData
{
    internal class XmlImporter
    {
        private ApiClientProvider ApiClientProvider { get; }

        /// <inheritdoc />
        public XmlImporter(ILoggerFactory loggerProvider, ApiClientProvider apiClientProvider, ILogger<XmlImporter> logger)
        {
            ApiClientProvider = apiClientProvider;
            LoggerProvider = loggerProvider;
            Log = logger;
        }

        private ILogger Log { get; }

        public ILoggerFactory LoggerProvider { get; }

        public async Task Import(string source)
        {
            var client = await ApiClientProvider.CreateApiClient();
            var identifierDefinitions = client.IdentityStoreMetadata();
            var evidenceDefinitions = client.ConsentStoreMetadata();
            
            var xmlParser = new XmlParser(LoggerProvider.CreateLogger<XmlParser>(), identifierDefinitions, evidenceDefinitions);
            using (var xmlReader = XmlReader.Create(source))
            {
                foreach (var person in xmlParser.GetPeople(xmlReader))
                {
                    var api = await ApiClientProvider.CreateApiClient();
                
                    using(Log.BeginScope(person))
                    {
                        var personId = api.PutPerson(person.PersonSpecification);
                    
                        Log.LogDebug("Person Id is {personId}", personId);
                    
                        //TODO: handle null personId - why would this happen?

                        if (!person.ConsentSpecifications.Any())
                        {
                            Log.LogDebug("No consents provided");
                            continue;
                        }
                    

                        foreach (var consent in person.ConsentSpecifications)
                        {
                            var givenBy = api.FindPerson(consent.GivenBy);

                            if (givenBy == null)
                            {
                                Log.LogTrace("Cannot find person who gave consent - {@specification}", new object[] { consent.GivenBy });
                                Log.LogError("Cannot find person who gave consent");
                                throw new NotImplementedException("Cannot find ");
                            }
                            var existingSubject = api.FindBySubjectId(consent.StudyId, personId.PersonId);

                            var subjectIdentifier =
                                existingSubject == null
                                    ? api.Generate(consent.StudyId)
                                    : existingSubject.SubjectIdentifier;
                        
                            api.PutConsent(
                                new ConsentSpecification(
                                    consent.StudyId,
                                    subjectIdentifier,
                                    personId.PersonId, 
                                    consent.DateGiven,
                                    consent.Evidence,
                                    givenBy.PersonId));
                        }
                    
                    }

                }
            }
        }
    }
}