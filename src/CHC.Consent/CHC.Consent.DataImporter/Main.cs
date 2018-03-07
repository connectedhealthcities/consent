using System;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.CommandLineUtils;

namespace CHC.Consent.DataImporter
{
    public static class Program
    {
        public static void Main(string[] args)
        {
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
            
        }

        private static void ShowError(string message, CommandLineApplication command)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
            command.ShowHelp();
        }
    }
}