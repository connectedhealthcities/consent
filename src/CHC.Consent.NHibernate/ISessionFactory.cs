using System;
using CHC.Consent.Common.Utils;
using NHibernate;
using NHibernate.Dialect.Function;

namespace CHC.Consent.NHibernate
{
    public interface ISessionFactory
    {
        ISession StartSession();
    }

    public static class SessionFactoryExtensions
    {
        public static T AsTransaction<T>(this ISessionFactory factory, Func<ISession, T> run)
        {
            using (var session = factory.StartSession())
            using (var tx = session.BeginTransaction())
            {
                var result = run(session);
                tx.Commit();
                return result;
            }
        }

        public static void AsTransaction(this ISessionFactory factory, Action<ISession> run)
        {
            factory.AsTransaction(
                _ =>
                {
                    run(_);
                    return Unit.Value;
                });
        }
    }
}