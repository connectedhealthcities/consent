using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using CHC.Consent.Api.Client;
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
        
        [Option( ShortName = "f", LongName = "fields", Description = "comma separated list of fields to output")]
        public string FieldNames { get; [UsedImplicitly] set; }

        /// <inheritdoc />
        public ExportCommand(ApiClientProvider apiClientProvider)
        {
            this.apiClientProvider = apiClientProvider;
        }

        [UsedImplicitly]
        protected async Task OnExecuteAsync()
        {
            IApi apiClient = await apiClientProvider.CreateApiClient();
            TextWriter CreateOutput() => System.IO.File.CreateText(File);

            await new CsvExporter(apiClient, FieldNames?.Split(',') ?? Array.Empty<string>())
                .Export(StudyId, CreateOutput);
        }
    }
}