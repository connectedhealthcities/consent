using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using NHibernate;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace CHC.Consent.NHibernate.Tests
{
    public class DatabaseFixture : IDisposable, ISessionFactory
    {
        private static int everSetup = 0;
        private bool setup = false;
        private Configuration configuration;
        private string connectionString = $@"Data Source=(LocalDB)\.;Integrated Security=True;";

        ISession  ISessionFactory.StartSession() => StartSession();
        
        public ISession StartSession(ITestOutputHelper output=null)
        {
            if (!setup)
            {
                if (Interlocked.Exchange(ref everSetup, 1) == 1)
                {
                    throw new InvalidOperationException("The database fixture has already been setup");
                }

                setup = true;
                var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "nhibnernate.mdf");


                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();


                    new SqlCommand(
                            $@"IF db_id('nhibernate_tests') is not null
                    BEGIN
                        DROP DATABASE nhibernate_tests;
                    END

                        CREATE DATABASE nhibernate_tests on (name='nhibernate_tests', filename='{dbPath.Replace("'", "''")}')",
                            connection).ExecuteNonQuery();
                }
                connectionString += "Initial Catalog=nhibernate_tests";
                
                configuration = new Configuration(Configuration.SqlServer(connectionString));
                
                configuration.Create(output == null ? (Action<string>)null : output.WriteLine, execute:true);
            }

            return configuration.StartSession();
        }


        public void Dispose()
        {
            /*if (configuration != null)
            {
                configuration.DropSchema(execute:true);
                
                
                configuration = null;
            }*/
        }
    }
}