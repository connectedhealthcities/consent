using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using CHC.Consent.NHibernate;
using CHC.Consent.NHibernate.Configuration;
using NHibernate;
using ISessionFactory = CHC.Consent.NHibernate.ISessionFactory;
using CHC.Consent.Utils;

namespace CHC.Consent.Testing.NHibernate
{
    public class DatabaseFixture : IDisposable, ISessionFactory
    {
        private static int everSetup = 0;
        private bool setup = false;
        private Configuration configuration;
        private string connectionString = $@"Data Source=(LocalDB)\.;Integrated Security=True;";

        ISession  ISessionFactory.StartSession() => StartSession();
        public UnitOfWorkFactory UnitOfWorkFactory { get; }
        /// <inheritdoc />
        public DatabaseFixture()
        {
            UnitOfWorkFactory = new UnitOfWorkFactory(this);
        }

        public T AsUnitOfWork<T>(Func<UnitOfWork, T> run)
        {
            using (var uow = UnitOfWorkFactory.Start())
            {
                return run(uow);
            }
        }

        public ISession SessionProvider()
        {
            return UnitOfWorkFactory.GetCurrentUnitOfWork().GetSession();
        }

        public T InTransactionalUnitOfWork<T>(Func<ISession, T> doWork)
        {
            return AsUnitOfWork(uow => uow.GetSession().AsTransaction(doWork));
        }

        public void InTransactionalUnitOfWork(Action<ISession> doWork) => InTransactionalUnitOfWork(doWork.AsUnitFunc());

        public T InTransactionalUnitOfWork<T>(Func<T> doWork) => InTransactionalUnitOfWork(doWork.IgnoreParams<ISession, T>());
        

        public ISession StartSession(Action<string> output=null)
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

                (output ?? Do.Nothing)(configuration.HbmXml);

                configuration.Create(output, execute:true);
            }

            var session = configuration.StartSession();

            return session;
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