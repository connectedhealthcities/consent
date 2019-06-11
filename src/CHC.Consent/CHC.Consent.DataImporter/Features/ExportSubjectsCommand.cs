using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CHC.Consent.Api.Client;
using CsvHelper;
using JetBrains.Annotations;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace CHC.Consent.DataImporter.Features
{
    [Command("export-subjects")]
    public class ExportSubjectsCommand
    {
        private ApiClientProvider apiClientProvider;
        private ILogger<ExportSubjectsCommand> loggerFactory;

        [Required, Argument(0, Description = "Study Id")]
        public long StudyId { get; set; }
        
        [Required, LegalFilePath, Argument(1, Description = "file to write to" )]
        public string FileName { get; set; }

         /// <inheritdoc />
        public ExportSubjectsCommand(ApiClientProvider apiClientProvider, ILogger<ExportSubjectsCommand> logger)
        {
            this.apiClientProvider = apiClientProvider;
            this.loggerFactory = logger;
        }
        
        [UsedImplicitly]
        protected async Task OnExecuteAsync()
        {
            var api = await apiClientProvider.CreateApiClient();

            var subjects = await api.GetSubjectsForStudyAsync(StudyId);

            using (var writer = new CsvWriter(
                new StreamWriter(File.Open(FileName, FileMode.OpenOrCreate, FileAccess.Write), Encoding.UTF8)))
            {
                writer.WriteRecords(subjects.Select(_ => new {SubjectId = _.SubjectIdentifier, _.LastWithdrawalDate}));
            }
        }
    }
}