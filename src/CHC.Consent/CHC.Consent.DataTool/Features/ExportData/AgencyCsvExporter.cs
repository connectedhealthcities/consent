using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CHC.Consent.Api.Client;
using CHC.Consent.Api.Client.Models;
using Microsoft.Extensions.Logging;

namespace CHC.Consent.DataTool.Features.ExportData
{
    public class AgencyCsvExporter : CsvExporter
    {
        public AgencyCsvExporter(IApi apiClient, string agencyName, ILogger<AgencyCsvExporter> logger)
        {
            ApiClient = apiClient;
            AgencyName = agencyName;
            Logger = logger;
        }

        private IApi ApiClient { get; }
        private string AgencyName { get; }
        public ILogger<AgencyCsvExporter> Logger { get; }

        /// <inheritdoc />
        public override async Task Export(long studyId, Func<TextWriter> createOutput)
        {
            var agencyInfo = await ApiClient.GetAgencyIdentifiersAndFieldNamesMetadataAsync(AgencyName);

            Logger.LogInformation("Writing fields {@fields}", agencyInfo.Agency.Fields);
            var subjects = await ApiClient.GetConsentedSubjectsForStudyAsync(studyId);

            var details = await Task.WhenAll(
                subjects.Select(
                    GetDetailsForPerson));

            new AgencySubjectWriter(createOutput)
                .Write(
                    agencyInfo.Identifiers,
                    FieldNameList.Split(agencyInfo.Agency.Fields),
                    details.Select(_ => new AgencyPersonDtoWrapper(_)));
        }

        private Task<AgencyPersonDto> GetDetailsForPerson(StudySubject studySubject)
        {
            Logger.LogDebug("Getting details for {@Person}", studySubject.PersonId);
            return ApiClient.GetPersonForAgencyAsync(AgencyName, studySubject.PersonId.Id);
        }

        private class AgencySubjectWriter : CsvWriterBase<AgencyPersonDtoWrapper>
        {
            /// <inheritdoc />
            public AgencySubjectWriter(Func<TextWriter> createOutputWriter) : base(createOutputWriter)
            {
            }

            /// <inheritdoc />
            protected override string GetId(AgencyPersonDtoWrapper haveIdentifiers) => haveIdentifiers.Id;
        }

        private struct AgencyPersonDtoWrapper : IHaveIdentifiers
        {
            private readonly AgencyPersonDto agencyPerson;

            public AgencyPersonDtoWrapper(AgencyPersonDto agencyPerson)
            {
                this.agencyPerson = agencyPerson;
            }

            public string Id => agencyPerson.Id;

            /// <inheritdoc />
            public IEnumerable<IIdentifierValueDto> Identifiers => agencyPerson.IdentifierValueDtos;
        }
    }
}