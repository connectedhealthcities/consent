using System;
using System.Reflection;
using CHC.Consent.NHibernate.Configuration;
using Oakton;

namespace CHC.Consent.Tools.SchemaTool
{
    public class CreateCommand : Oakton.OaktonCommand<SchemaInput>
    {
        /// <inheritdoc />
        public override bool Execute(SchemaInput input)
        {
            new Configuration(Configuration.SqlServer(input.ConnectionString)).Create(Console.WriteLine, execute:input.ExecuteFlag);

            return true;
        }
    }

    public class SchemaInput
    {
        public string ConnectionString { get; set; }
        public bool ExecuteFlag { get; set; }
    }

    public class UpdateCommand : OaktonCommand<SchemaInput>
    {
        /// <inheritdoc />
        public override bool Execute(SchemaInput input)
        {
            new Configuration(Configuration.SqlServer(input.ConnectionString)).Update(Console.WriteLine, execute:input.ExecuteFlag);

            return true;
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            var executor = CommandExecutor.For(_ =>
            {
                // Automatically discover and register
                // all OaktonCommand's in this assembly
                _.RegisterCommands(typeof(Program).GetTypeInfo().Assembly);
            });

            executor.Execute(args);

        }
    }
}
