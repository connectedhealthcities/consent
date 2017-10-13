using System;
using CHC.Consent.Utils;
using NHibernate;

namespace CHC.Consent.NHibernate
{
    public static class SessionFactoryExtensions
    {
        public static T AsTransaction<T>(this ISession session, Func<ISession, T> run)
        {
            using (var tx = session.BeginTransaction())
            {
                var result = run(session);
                tx.Commit();
                return result;
            }
        }

        public static void AsTransaction(this ISession session, Action<ISession> run)
            => session.AsTransaction(run.AsUnitFunc());
    }
}