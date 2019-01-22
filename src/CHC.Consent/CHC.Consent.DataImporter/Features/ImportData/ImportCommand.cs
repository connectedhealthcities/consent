using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CHC.Consent.Api.Client;
using JetBrains.Annotations;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;

namespace CHC.Consent.DataImporter.Features.ImportData
{
    [Command("import")]
    class ImportCommand
    {
        private readonly ILoggerFactory loggerFactory;
        private readonly XmlImporter xmlImporter;

        [Required, LegalFilePath, Argument(0, "file", "file to import")]
        public string File { get; set; }

        
        
        /// <inheritdoc />
        public ImportCommand(ILoggerFactory loggerFactory, ApiClientProvider apiClientProvider)
        {
            this.loggerFactory = loggerFactory;
            xmlImporter = new XmlImporter(loggerFactory, apiClientProvider, loggerFactory.CreateLogger<XmlImporter>());
        }

        [UsedImplicitly]
        private async Task<int> OnExecuteAsync()
        {
            
            
            await Import(File);

            return 0;
        }
        
        private async Task Import(string filePath)
        {
            await xmlImporter.Import(filePath);

        }
    }
}