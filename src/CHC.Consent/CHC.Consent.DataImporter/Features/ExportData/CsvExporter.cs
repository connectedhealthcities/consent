using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CHC.Consent.Api.Client;
using CHC.Consent.Api.Client.Models;

namespace CHC.Consent.DataImporter.Features.ExportData
{
    public class CsvExporter
    {
        private readonly string[][] fieldNames;

        public CsvExporter(IApi apiClient, string[] fieldNames)
        {
            this.fieldNames = FieldNameList.Split(fieldNames);
            ApiClient = apiClient;
        }

        private IApi ApiClient { get; set; }

        public async Task Export(long studyId, Func<TextWriter> outputStream)
        {
            var definitions = await GetIdentifierDefinitions();

            CheckFieldNames(definitions);

            var studySubjects = await GetSubjectIdentifiersAndIdentityValues(studyId);

            new StudySubjectCsvWriter(outputStream).Write(definitions, fieldNames, studySubjects);
        }

        private Task<IList<IdentifierDefinition>> GetIdentifierDefinitions() =>
            ApiClient.GetIdentityStoreMetadataAsync();

        private async Task<IEnumerable<StudySubjectWithIdentifiers>> GetSubjectIdentifiersAndIdentityValues(
            long studyId)
        {
            var consentedSubjectsForStudyAsync = await ApiClient.GetConsentedSubjectsForStudyAsync(studyId);
            return
                await Task.WhenAll(
                    consentedSubjectsForStudyAsync
                        .Select(
                            async studySubject =>
                                new StudySubjectWithIdentifiers
                                {
                                    subjectIdentifier = studySubject.SubjectIdentifier,
                                    identifiers = await GetIdentifiers(studySubject)
                                })
                );
        }

        private async Task<IList<IIdentifierValueDto>> GetIdentifiers(StudySubject studySubject)
        {
            return fieldNames.Any()
                ? await ApiClient.GetPersonAsync(studySubject.PersonId.Id.Value)
                : Array.Empty<IIdentifierValueDto>();
        }

        private void CheckFieldNames(IList<IdentifierDefinition> definitions)
        {
            var definitionNames = FieldNameList.CreateFromDefinitions(definitions);
            var invalidFieldNames = definitionNames.Except(FieldNameList.FullFieldNames(fieldNames)).ToArray();
            if (invalidFieldNames.Any())
            {
                throw new InvalidOperationException($"Invalid field names: '{string.Join("', '", invalidFieldNames)}'");
            }
        }
    }

}