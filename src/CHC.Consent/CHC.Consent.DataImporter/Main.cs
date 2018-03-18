using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Xml;
using CHC.Consent.Api.Client;
using CHC.Consent.Api.Client.Models;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Rest;
using Serilog;
using Serilog.Events;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace CHC.Consent.DataImporter
{
    public static class Program
    {
        private static ILoggerFactory _loggerFactory;

        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Is(LogEventLevel.Verbose)
                .CreateLogger();
            
            _loggerFactory = new LoggerFactory().AddSerilog();
            
            ServiceClientTracing.IsEnabled = true;
            ServiceClientTracing.AddTracingInterceptor(
                new LoggerServiceClientTracingIntercepter(
                    _loggerFactory.CreateLogger("Http"),
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

        private static void Import(string filePath)
        {
            new XmlImporter(_loggerFactory).Import(filePath);

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
        private readonly ILoggerFactory loggerProvider;

        /// <inheritdoc />
        public XmlImporter(ILoggerFactory loggerProvider)
        {
            this.loggerProvider = loggerProvider;
            this.Log = loggerProvider.CreateLogger<XmlImporter>();
        }

        public ILogger Log { get; set; }

        public void Import(string source)
        {
            using (var xmlReader = XmlReader.Create(source))
            {
                foreach (var person in new XmlParser(loggerProvider.CreateLogger<XmlParser>()).GetPeople(xmlReader))
                {
                    var api = new Api.Client.Api(new Uri("http://localhost:5000/"), new HttpClientHandler{AllowAutoRedirect = false});
                
                    using(Log.BeginScope(person))
                    {
                        var personId = api.IdentitiesPut(person.PersonSpecification);
                    
                        Log.LogDebug("Person Id is {personId}", personId);
                    
                        //TODO: handle null personId - why would this happen?

                        if (!person.ConsentSpecifications.Any())
                        {
                            Log.LogDebug("No consents provided");
                            continue;
                        }
                    

                        foreach (var consent in person.ConsentSpecifications)
                        {
                            var givenBy = api.IdentitiesSearchPost(consent.GivenBy);

                            if (givenBy == null)
                            {
                                Log.LogTrace("Cannot find person who gave consent - {@specification}", new object[] { consent.GivenBy });
                                Log.LogError("Cannot find person who gave consent");
                                throw new NotImplementedException("Cannot find ");
                            }
                            var existingSubject = api.StudiesByStudyIdSubjectsGet(consent.StudyId, personId.PersonId);

                            var subjectIdentifier =
                                existingSubject == null
                                    ? api.SubjectIdentifiersByStudyIdPost(consent.StudyId)
                                    : existingSubject.SubjectIdentifier;
                        
                            api.ConsentPut(
                                new ConsentSpecification(
                                    consent.StudyId,
                                    subjectIdentifier,
                                    personId.PersonId, 
                                    consent.DateGiven,
                                    consent.Evidence,
                                    givenBy.PersonId,
                                    consent.CaseId));
                        }
                    
                    }

                }
            }
        }
    }
}