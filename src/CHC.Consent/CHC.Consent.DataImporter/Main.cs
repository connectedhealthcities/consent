using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using CHC.Consent.Api.Client;
using CHC.Consent.Api.Client.Models;
using CHC.Consent.Common.Identity;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
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
        public class ApiConfiguration
        {
            public string BaseUrl { get; set; }
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
        }
        private static ILoggerFactory _loggerFactory;
        private static IConfigurationRoot configuration;

        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Is(LogEventLevel.Verbose)
                .CreateLogger();
            
            _loggerFactory = new LoggerFactory().AddSerilog();
            
            ServiceClientTracing.IsEnabled = true;
            ServiceClientTracing.AddTracingInterceptor(
                new LoggerServiceClientTracingInterceptor(
                    _loggerFactory.CreateLogger("Http"),
                    LogLevel.Trace));


            configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false)
                .AddEnvironmentVariables()
                .AddCommandLine(args, new Dictionary<string, string>{ ["-url"] = "Api_BaseUrl", ["-client_id"] = "Api_ClientId", ["-secret"] = "Api_ClientSecret"})
                .Build();


            var application = new CommandLineApplication()
                .Command(
                    "import",
                    config =>
                    {
                        var file = config.Argument("file", "file to import");

                        config.HelpOption("--help");


                        config.OnExecute(
                            async () =>
                            {
                                if (string.IsNullOrEmpty(file.Value))
                                {
                                    ShowError("Please specify the file to import", config);
                                }
                                else
                                {
                                    Console.WriteLine("Import {0}", file.Value);
                                }

                                await Import(file.Value, configuration.GetValue<ApiConfiguration>("Api"));

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

        private static async Task Import(string filePath, ApiConfiguration apiConfiguration)
        {
            await new XmlImporter(_loggerFactory, apiConfiguration).Import(filePath);

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
        private Program.ApiConfiguration ApiConfiguration { get; }
        private readonly ILoggerFactory loggerProvider;

        /// <inheritdoc />
        public XmlImporter(ILoggerFactory loggerProvider, Program.ApiConfiguration apiConfiguration)
        {
            ApiConfiguration = apiConfiguration;
            this.loggerProvider = loggerProvider;
            this.Log = loggerProvider.CreateLogger<XmlImporter>();
        }

        public ILogger Log { get; set; }

        public async Task Import(string source)
        {
            var client = await CreateApiClient();
            var identifierDefinitions = client.IdentityStoreMetadata();
            
            var xmlParser = new XmlParser(loggerProvider.CreateLogger<XmlParser>(), identifierDefinitions);
            using (var xmlReader = XmlReader.Create(source))
            {
                foreach (var person in xmlParser.GetPeople(xmlReader))
                {
                    var api = await CreateApiClient();
                
                    using(Log.BeginScope(person))
                    {
                        var personId = api.PutPerson(person.PersonSpecification);
                    
                        Log.LogDebug("Person Id is {personId}", personId);
                    
                        //TODO: handle null personId - why would this happen?

                        if (!person.ConsentSpecifications.Any())
                        {
                            Log.LogDebug("No consents provided");
                            continue;
                        }
                    

                        foreach (var consent in person.ConsentSpecifications)
                        {
                            var givenBy = api.FindPerson(consent.GivenBy);

                            if (givenBy == null)
                            {
                                Log.LogTrace("Cannot find person who gave consent - {@specification}", new object[] { consent.GivenBy });
                                Log.LogError("Cannot find person who gave consent");
                                throw new NotImplementedException("Cannot find ");
                            }
                            var existingSubject = api.FindBySubjectId(consent.StudyId, personId.PersonId);

                            var subjectIdentifier =
                                existingSubject == null
                                    ? api.Generate(consent.StudyId)
                                    : existingSubject.SubjectIdentifier;
                        
                            api.PutConsent(
                                new ConsentSpecification(
                                    consent.StudyId,
                                    subjectIdentifier,
                                    personId.PersonId, 
                                    consent.DateGiven,
                                    consent.Evidence,
                                    givenBy.PersonId));
                        }
                    
                    }

                }
            }
        }

        private async Task<Api.Client.Api> CreateApiClient()
        {
            var httpClient = new HttpClient(){ BaseAddress = new Uri(ApiConfiguration.BaseUrl) };
            var discoveryResponse = await httpClient.GetDiscoveryDocumentAsync();
            EnsureSuccess(discoveryResponse);
            var tokenResponse = await httpClient.RequestClientCredentialsTokenAsync(
                new ClientCredentialsTokenRequest
                {
                    Scope = "api",
                    ClientId = ApiConfiguration.ClientId,
                    ClientSecret = ApiConfiguration.ClientSecret,
                    GrantType = OidcConstants.GrantTypes.ClientCredentials
                });
            EnsureSuccess(tokenResponse);                
                
            return new Api.Client.Api(new Uri(ApiConfiguration.BaseUrl), new TokenCredentials(tokenResponse.AccessToken), new HttpClientHandler{AllowAutoRedirect = false});
        }

        private static void EnsureSuccess(dynamic tokenResponse)
        {
            if (tokenResponse.IsError) throw new Exception(tokenResponse.Error);
        }
    }
}