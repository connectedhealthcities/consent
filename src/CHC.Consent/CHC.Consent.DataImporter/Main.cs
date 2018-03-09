using System;
using System.IO;
using System.Net.Http;
using CHC.Consent.Api.Client;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Rest;
using Serilog;
using Serilog.Events;

namespace CHC.Consent.DataImporter
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Is(LogEventLevel.Verbose)
                .CreateLogger();
            
            var loggerFactory = new LoggerFactory().AddSerilog();
            
            ServiceClientTracing.IsEnabled = true;
            ServiceClientTracing.AddTracingInterceptor(
                new LoggerServiceClientTracingIntercepter(
                    loggerFactory.CreateLogger<Api.Client.Api>(),
                    LogLevel.Trace));
            
            
            var application = new CommandLineApplication();

            var import = application.Command(
                "import",
                config =>
                {
                    var file = config.Argument("file", "file to import");
                    
                    config.HelpOption("--help");


                    config.OnExecute(
                        () =>
                        {
                            if (string.IsNullOrEmpty(file.Value))
                            {
                                ShowError("Please specify the file to import", config);
                            }
                            else
                            {
                                Console.WriteLine("Import {0}", file.Value);    
                            }

                            Import(file.Value);
                            
                            return 0;
                        });
                });

            application.HelpOption("-? | -h | --help");

            try
            {
                application.Execute(args);
            }
            catch (CommandParsingException e)
            {
                ShowError(e.Message, e.Command);
            }
            
        }

        private static void Import(string fileValue)
        {
            using (var streamReader = new StreamReader(File.OpenRead(fileValue)))
            {
                new XmlImporter().Import(streamReader);
            }
        }

        private static void ShowError(string message, CommandLineApplication command)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
            command.ShowHelp();
        }
    }

    internal class XmlImporter
    {
        public void Import(StreamReader source)
        {
            foreach (var person in new XmlParser().GetPeople(source))
            {
                var api = new Api.Client.Api(new Uri("http://localhost:5000/"), new HttpClientHandler{AllowAutoRedirect = false});
                
                var response = api
                    .IdentitiesPutWithHttpMessagesAsync(person).GetAwaiter().GetResult();
            }
        }
    }
}