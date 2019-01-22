using System.Collections.Generic;
using System.Threading.Tasks;
using CHC.Consent.DataImporter.Features.ImportData;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.Hosting.CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace CHC.Consent.DataImporter
{
    [Command("chc-consent"), Subcommand(typeof(ImportCommand)), HelpOption]
    public class Program
    {
        public static Task<int> Main(string[] args)
        {
            Serilog.Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Is(LogEventLevel.Verbose)
                .CreateLogger();

            return new HostBuilder()
                .ConfigureAppConfiguration(
                    c =>
                        c.AddJsonFile("appsettings.json", false)
                            .AddEnvironmentVariables()
                            .AddCommandLine(
                                args,
                                new Dictionary<string, string>
                                {
                                    ["-url"] = "Api_BaseUrl", 
                                    ["-client_id"] = "Api_ClientId",
                                    ["-secret"] = "Api_ClientSecret"
                                })
                )
                .ConfigureLogging((c, l) =>  l.AddSerilog().SetMinimumLevel(LogLevel.Trace))
                .ConfigureServices(
                    (context, services) =>
                        services
                            .AddSingleton(context.Configuration.GetSection("Api").Get<ApiConfiguration>())
                            .AddSingleton<ApiClientProvider>()
                        )
                .RunCommandLineApplicationAsync<Program>(args);
        }

        private int OnExecuteAsync(CommandLineApplication app)
        {
            // this shows help even if the --help option isn't specified
            app.ShowHelp();
            return 1;
        }

    }
}