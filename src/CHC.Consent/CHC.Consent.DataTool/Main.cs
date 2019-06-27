using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CHC.Consent.Api.Client.Models;
using CHC.Consent.DataTool.Features;
using CHC.Consent.DataTool.Features.ExportData;
using CHC.Consent.DataTool.Features.ImportData;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.Hosting.CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace CHC.Consent.DataTool
{
    [Command("chc-consent"), Subcommand(typeof(ImportCommand), typeof(ExportCommand), typeof(ExportSubjectsCommand)), HelpOption]
    public class Program
    {
        public static Task<int> Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                /*.WriteTo.Console(formatter:new JsonFormatter(renderMessage:true))*/
                .WriteTo.Console(outputTemplate:"[{Level:u}] {Message:lj} {Exception} {Properties:j}{NewLine}",theme: AnsiConsoleTheme.Literate)
                .Enrich.FromLogContext()
                .Destructure.With<DefinitionDestructor>()
                .MinimumLevel.Is(LogEventLevel.Verbose)
                .CreateLogger();

            
            return new HostBuilder()
                .ConfigureAppConfiguration(
                    c =>
                    {
                        Log.Information(
                            "Using settings from {appSettingsPath}",
                            c.GetFileProvider().GetFileInfo("appsettings.json").PhysicalPath);
                        c.AddJsonFile("appsettings.json", false)
                            .AddEnvironmentVariables();
                    })
                
                .ConfigureServices(
                    (context, services) =>
                        services
                            .AddSingleton(context.Configuration.GetSection("Api").Get<ApiConfiguration>())
                            .AddSingleton<ApiClientProvider>()
                            .AddSingleton(Log.Logger)
                            .AddSingleton<IUnhandledExceptionHandler,ExceptionLogger>()
                        )
                .UseSerilog()
                .RunCommandLineApplicationAsync<Program>(args);
            
            
        }

        private int OnExecuteAsync(CommandLineApplication app)
        {
            // this shows help even if the --help option isn't specified
            app.ShowHelp();
            return 1;
        }

    }

    public class DefinitionDestructor : IDestructuringPolicy
    {
        /// <inheritdoc />
        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
        {
            result = null;
            if (!(value is IDefinition definition)) return false;
            result = propertyValueFactory.CreatePropertyValue($"{definition.SystemName}:{definition.Type?.SystemName}");
            return true;
        }
    }
}