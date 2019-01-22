using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CHC.Consent.Api.Client;
using McMaster.Extensions.CommandLineUtils;

namespace CHC.Consent.DataImporter.Features.ExportData
{
    [Command("export")]
    public class ExportCommand
    {
        private readonly ApiClientProvider apiClientProvider;

        [Required, LegalFilePath, Argument(1, "output-file", "file to output")]
        public string File { get; set; }
        
        [Required, Argument(0, "study-id", "the id of the study to export")]
        public long StudyId { get; set; }

        /// <inheritdoc />
        public ExportCommand(ApiClientProvider apiClientProvider)
        {
            this.apiClientProvider = apiClientProvider;
        }

        protected async Task OnExecuteAsync()
        {
            var apiClient = await apiClientProvider.CreateApiClient();
            var studySubjects = apiClient.GetConsentedSubjectsForStudy(StudyId);
            using (var output = System.IO.File.CreateText(File))
            {
                foreach (var studySubject in studySubjects)
                {
                    await output.WriteLineAsync(studySubject.SubjectIdentifier);
                }
            }
        }
    }
}