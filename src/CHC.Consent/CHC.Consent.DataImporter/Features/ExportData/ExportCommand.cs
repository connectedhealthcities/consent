using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
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
    public class StudySubjectWithIdentifiers
    {
        public string subjectIdentifier { get; set; }
        public IList<IIdentifierValueDto> identifiers { get; set; }
    }

    public class CsvExporter
    {
        public CsvExporter(IApi apiClient)
        {
            ApiClient = apiClient;
        }

        private IApi ApiClient { get; set; }

        public async Task Export(long studyId, StreamWriter outputWriter)
        {
            var definitions = await ApiClient.GetIdentityStoreMetadataAsync();
            var studySubjects = ApiClient
                .GetConsentedSubjectsForStudy(studyId)
                .Select(
                    studySubject =>
                        new StudySubjectWithIdentifiers
                        {
                            subjectIdentifier = studySubject.SubjectIdentifier,
                            identifiers = ApiClient.GetPerson(studySubject.PersonId.Id.Value)
                        })
                .ToArray();


            Write(definitions, studySubjects, outputWriter);
        }

        public virtual void Write(
            ICollection<IdentifierDefinition> definitions, 
            IEnumerable<StudySubjectWithIdentifiers> studySubjects,
            TextWriter outputWriter)
        {
            var outputFormatter = new ValueOutputFormatter(definitions);
            using (var csv = new CsvWriter(outputWriter))
            {
                WriteHeader(csv, definitions);

                foreach (var subject in studySubjects)
                {
                    WriteRecord(csv, outputFormatter, subject.subjectIdentifier, subject.identifiers);
                }
            }
        }

        private static void WriteRecord(
            IWriter output,
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
            output.WriteField("id");
            foreach (var name in FieldNames(definitions))
            {
                output.WriteField(name);
            }

            output.NextRecord();
        }

        private static IEnumerable<string> FieldNames(IEnumerable<IdentifierDefinition> definitions) =>
            new GatherOutputNames(definitions);
    }

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
            var outputWriter = System.IO.File.CreateText(File);
            
            await new CsvExporter(apiClient).Export(StudyId, outputWriter);
        }
    }
}