using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;
using Xunit.Abstractions;

namespace CHC.Consent.EFCore.Tests
{
    [Collection(DatabaseCollection.Name)]
    public abstract class DbTests : IDisposable
    {
        protected IDbContextTransaction transaction;
        protected ITestOutputHelper outputHelper;
        protected DatabaseFixture fixture;
        private protected ConsentContext createContext;
        private protected ConsentContext updateContext;
        private protected ConsentContext readContext;
        protected ConsentContext Context { get; }

        protected DbTests(ITestOutputHelper outputHelper, DatabaseFixture fixture)
        {
            this.outputHelper = outputHelper;
            this.fixture = fixture;
            Context = fixture.GetContext(outputHelper);
            transaction = Context.Database.BeginTransaction();

            createContext = CreateNewContextInSameTransaction();
            updateContext = CreateNewContextInSameTransaction();
            readContext = CreateNewContextInSameTransaction();
        }

        protected ConsentContext CreateNewContextInSameTransaction()
        {
            var newContext = fixture.GetContext(outputHelper, Context.Database.GetDbConnection());
            newContext.Database.UseTransaction(transaction.GetDbTransaction());
            return newContext;
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            try {transaction?.Rollback();} catch(InvalidOperationException) { }
            transaction?.Dispose();
            Context?.Dispose();
            createContext?.Dispose();
            updateContext?.Dispose();
            readContext?.Dispose();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}