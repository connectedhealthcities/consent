using System;
using NHibernate;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace CHC.Consent.NHibernate.Tests
{
    public class DatabaseFixture : IDisposable
    {
        private bool setup = false;
        private Configuration configuration;


        public ISession StartSession(ITestOutputHelper output=null)
        {
            if (!setup)
            {

                setup = true;
                configuration = new Configuration(
                    Configuration.SqlServer(@"Data Source=(localdb)\.;Integrated Security=true"));

                configuration.Create(output == null ? (Action<string>)null : output.WriteLine, execute:true);
            }

            return configuration.StartSession();
        }

        public void AsTransaction(Action<ISession> run)
        {
            using(var session = StartSession())
            using (var tx = session.BeginTransaction())
            {
                run(session);
                
                tx.Commit();
            }
        }

        public void Dispose()
        {
            if (configuration != null)
            {
                configuration.DropSchema(execute:true);
                configuration = null;
            }
        }
    }
}