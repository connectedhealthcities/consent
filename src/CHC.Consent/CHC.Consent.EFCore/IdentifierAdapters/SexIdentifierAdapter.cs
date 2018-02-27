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
    public class SexIdentifierAdapter : SingleValueIdentifierAdapter<SexIdentifier, Sex?>
    {
        /// <inheritdoc />
        public override IQueryable<PersonEntity> Filter(
            IQueryable<PersonEntity> people,
            SexIdentifier value,
            IStoreProvider stores)  =>
            people.Where(_ => _.Sex == value.Sex);

        /// <inheritdoc />
        protected override string IdentifierName => "Sex";

        /// <inheritdoc />
        public override void SetValue(PersonEntity entity, Sex? newValue) => entity.Sex = newValue;

        /// <inheritdoc />
        public override Sex? ExistingValue(PersonEntity entity) => entity.Sex;

        /// <inheritdoc />
        public override IEnumerable<SexIdentifier> Get(PersonEntity person, IStoreProvider stores)
        {
            return person.Sex == null ? Enumerable.Empty<SexIdentifier>() : new[] {new SexIdentifier(person.Sex)};
        }
    }
}