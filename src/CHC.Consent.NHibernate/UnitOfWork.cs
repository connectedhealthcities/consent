using System;
using NHibernate;

namespace CHC.Consent.NHibernate
{
    public class UnitOfWork : IDisposable
    {
        private readonly ISessionFactory sessionFactory;
        private readonly Action onDisposed;

        protected ISession CurrentSession { get; private set; }

        /// <inheritdoc />
        public UnitOfWork(ISessionFactory sessionFactory, Action onDisposed)
        {
            this.sessionFactory = sessionFactory;
            this.onDisposed = onDisposed;
        }

        public ISession GetSession()
        {
            return CurrentSession ?? (CurrentSession = sessionFactory.StartSession());
        }

        /// <inheritdoc />
        public void Dispose()
        {
            onDisposed();
            CurrentSession?.Dispose();
        }
    }
}