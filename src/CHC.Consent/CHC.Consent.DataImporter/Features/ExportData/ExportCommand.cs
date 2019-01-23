using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using CHC.Consent.Api.Client;
using CHC.Consent.Api.Client.Models;
using CsvHelper;
using JetBrains.Annotations;
using McMaster.Extensions.CommandLineUtils;

namespace CHC.Consent.DataImporter.Features.ExportData
{
    [Command("export")]
    public class ExportCommand
    {
        private readonly ApiClientProvider apiClientProvider;

        [Required, Argument(0, "study-id", "the id of the study to export")]
        // ReSharper disable once MemberCanBePrivate.Global
        public long StudyId { get; [UsedImplicitly]set; }

        [Required, LegalFilePath, Argument(1, "output-file", "file to output")]
        // ReSharper disable once MemberCanBePrivate.Global
        public string File { get; [UsedImplicitly]set; }

        /// <inheritdoc />
        public ExportCommand(ApiClientProvider apiClientProvider)
        {
            this.apiClientProvider = apiClientProvider;
        }

        protected async Task OnExecuteAsync()
        {
            IApi apiClient = await apiClientProvider.CreateApiClient();
            var definitions = await apiClient.GetIdentityStoreMetadataAsync();
            var studySubjects = apiClient
                .GetConsentedSubjectsForStudy(StudyId)
                .Select(
                    studySubject =>
                        (subjectIdentifier: studySubject.SubjectIdentifier,
                            identifiers: apiClient.GetPerson(studySubject.PersonId.Id.Value)))
                .ToArray();

            var outputFormatter = new ValueOutputFormatter(definitions);
            
            using (var output = new CsvWriter(System.IO.File.CreateText(File)))
            {
                WriteHeader(output, definitions);


                foreach (var (subjectIdentifier, identifierValues) in studySubjects)
                {
                    WriteRecord(output, outputFormatter, subjectIdentifier, identifierValues);
                }
            }
        }

        private static void WriteRecord(
            CsvWriter output,
            ValueOutputFormatter outputFormatter,
            string subjectIdentifier,
            IEnumerable<IIdentifierValueDto> identifiers)
        {
            output.WriteField(subjectIdentifier);
            outputFormatter.Write(identifiers, output);
            output.NextRecord();
        }

        private static void WriteHeader(IWriter output, IEnumerable<IdentifierDefinition> definitions)
        {
            output.WriteField("Id");
            foreach (var name in FieldNames(definitions))
            {
                output.WriteField(name);
            }

            output.NextRecord();
        }

        private static IEnumerable<string> FieldNames(IEnumerable<IdentifierDefinition> definitions) =>
            new GatherOutputNames(definitions);
    }
}