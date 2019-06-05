using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using CHC.Consent.Api.Client;
using JetBrains.Annotations;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace CHC.Consent.DataImporter.Features.ExportData
{
    [Command("export")]
    public class ExportCommand
    {
        private readonly ApiClientProvider apiClientProvider;
        private readonly ILoggerFactory loggerFactory;

        /// <inheritdoc />
        public ExportCommand(ApiClientProvider apiClientProvider, ILoggerFactory loggerFactory)
        {
            this.apiClientProvider = apiClientProvider;
            this.loggerFactory = loggerFactory;
        }

        [Required, Argument(0, "study-id", "the id of the study to export")]
        // ReSharper disable once MemberCanBePrivate.Global
        public long StudyId { get; [UsedImplicitly] set; }

        [Required, LegalFilePath, Argument(1, "output-file", "file to output")]
        // ReSharper disable once MemberCanBePrivate.Global
        public string File { get; [UsedImplicitly] set; }

        [Option(ShortName = "f", LongName = "fields", Description = "comma separated list of fields to output")]
        public (bool hasValue, string value) FieldNames { get; [UsedImplicitly] set; }


        [Option(ShortName = "a", LongName = "agency", Description = "Agency to request data for")]
        public (bool hasValue, string value) Agency { get; [UsedImplicitly] set; }

        [UsedImplicitly]
        ValidationResult OnValidate(ValidationContext args)
        {
            if (FieldNames.hasValue && Agency.hasValue)
                return new ValidationResult(
                    "Please provide agency OR fields, not both",
                    new[] {nameof(FieldNames), nameof(Agency)});

            return ValidationResult.Success;
        }

        [UsedImplicitly]
        protected async Task OnExecuteAsync()
        {
            IApi apiClient = await apiClientProvider.CreateApiClient();
            TextWriter CreateOutput() => System.IO.File.CreateText(File);

            var exporter = Agency.hasValue
                ? (CsvExporter) new AgencyCsvExporter(
                    apiClient,
                    Agency.value,
                    loggerFactory.CreateLogger<AgencyCsvExporter>())
                : new FieldsCsvExporter(apiClient, FieldNames.value?.Split(',') ?? Array.Empty<string>());
            await exporter.Export(StudyId, CreateOutput);
        }
    }
}