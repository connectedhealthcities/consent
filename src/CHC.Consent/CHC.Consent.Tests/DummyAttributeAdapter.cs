using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.Tests
{
    internal class DummyAttributeAdapter<TIdentifier> : IIdentifierFilter<TIdentifier>, IIdentifierUpdater<TIdentifier>, IIdentifierRetriever<TIdentifier> where TIdentifier : IPersonIdentifier
    {
        /// <inheritdoc />
        public IQueryable<PersonEntity> Filter(
            IQueryable<PersonEntity> people, TIdentifier value, IStoreProvider stores) => throw new NotImplementedException();

        /// <inheritdoc />
        public bool Update(PersonEntity person, TIdentifier value, IStoreProvider stores) => throw new NotImplementedException();

        /// <inheritdoc />
        public IEnumerable<TIdentifier> Get(PersonEntity person, IStoreProvider stores) => throw new NotImplementedException();
    }
}