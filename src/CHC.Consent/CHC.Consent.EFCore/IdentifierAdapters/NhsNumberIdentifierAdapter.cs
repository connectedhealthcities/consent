using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore.IdentifierAdapters
{
    public class NhsNumberIdentifierAdapter : SingleValueIdentifierAdapter<NhsNumberIdentifier, string>
    {
        /// <inheritdoc />
        public override IQueryable<PersonEntity> Filter(
            IQueryable<PersonEntity> people,
            NhsNumberIdentifier value,
            IStoreProvider stores) =>
            people.Where(_ => _.NhsNumber == value.Value);

        /// <inheritdoc />
        protected override string IdentifierName => "NHS Number";

        /// <inheritdoc />
        public override void SetValue(PersonEntity entity, string newValue) => entity.NhsNumber = newValue;

        /// <inheritdoc />
        public override string ExistingValue(PersonEntity entity) => entity.NhsNumber;

        /// <inheritdoc />
        public override IEnumerable<NhsNumberIdentifier> Get(PersonEntity person, IStoreProvider stores)
        {
            return person.NhsNumber == null
                ? Enumerable.Empty<NhsNumberIdentifier>()
                : new[] {new NhsNumberIdentifier(person.NhsNumber)};
        }
    }
}