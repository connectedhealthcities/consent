using System;
using System.Threading;

namespace CHC.Consent.NHibernate
{
    public class UnitOfWorkFactory
    {
        private readonly ISessionFactory sessionFactory;
        private readonly AsyncLocal<UnitOfWork> current = new AsyncLocal<UnitOfWork>();

        public UnitOfWork GetCurrentUnitOfWork()
        {
            if(current.Value == null) throw new InvalidOperationException("Not in a unit of work");
            return current.Value;
        }

        public UnitOfWork Start()
        {
            if(current.Value != null) throw new InvalidOperationException();

            return current.Value = new UnitOfWork(sessionFactory, ClearSession);
        }

        private void ClearSession()
        {
            current.Value = null;
        }

        /// <inheritdoc />
        public UnitOfWorkFactory(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }
    }
}