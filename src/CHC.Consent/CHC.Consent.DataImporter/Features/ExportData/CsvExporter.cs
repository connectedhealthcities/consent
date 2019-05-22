using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CHC.Consent.Api.Client;
using CHC.Consent.Api.Client.Models;
using CsvHelper;

namespace CHC.Consent.DataImporter.Features.ExportData
{
    public class CsvExporter
    {
        private readonly string[][] fieldNames;

        public CsvExporter(IApi apiClient, string[] fieldNames)
        {
            this.fieldNames = fieldNames.Select(_ => _.Split(FieldNameList.Separator)).ToArray();
            ApiClient = apiClient;
        }

        private IApi ApiClient { get; set; }

        public async Task Export(long studyId, Func<TextWriter> outputStream)
        {
            var definitions = await GetIdentifierDefinitions();

            CheckFieldNames(definitions);

            var studySubjects = await GetSubjectIdentifiersAndIdentityValues(studyId);

            Write(definitions, studySubjects, outputStream);
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
            var invalidFieldNames = definitionNames.Except(FullFieldNames()).ToArray();
            if (invalidFieldNames.Any())
            {
                throw new InvalidOperationException($"Invalid field names: '{string.Join("', '", invalidFieldNames)}'");
            }
        }

        private IEnumerable<string> FullFieldNames()
        {
            return fieldNames.Select(FieldNameList.Join);
        }

        public virtual void Write(
            ICollection<IdentifierDefinition> definitions, 
            IEnumerable<StudySubjectWithIdentifiers> studySubjects,
            Func<TextWriter> createOutputWriter)
        {
            
            var outputFormatter = new ValueOutputFormatter(definitions, fieldNames);
            using (var csv = new CsvWriter(createOutputWriter()))
            {
                WriteHeader(csv, definitions);

                foreach (var subject in studySubjects)
                {
                    WriteRecord(csv, outputFormatter, subject.subjectIdentifier, subject.identifiers);
                }
            }
        }

        private void WriteRecord(
            IWriter output,
            ValueOutputFormatter outputFormatter,
            string subjectIdentifier,
            IEnumerable<IIdentifierValueDto> values)
        {
            output.WriteField(subjectIdentifier);
            outputFormatter.Write(values, output);
            output.NextRecord();
        }

        private void WriteHeader(IWriter output, IEnumerable<IdentifierDefinition> definitions)
        {
            output.WriteField("id");
            foreach (var name in FullFieldNames())
            {
                output.WriteField(name);
            }

            output.NextRecord();
        }
    }
}