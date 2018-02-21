using System;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore.IdentifierAdapters
{
    public class SexIdentifierAdapter : SingleValueIdentifierAdapter<SexIdentifier, Sex?>
    {
        /// <inheritdoc />
        public override IQueryable<TPerson> Filter<TPerson>(
            IQueryable<TPerson> people,
            SexIdentifier value,
            IStoreProvider stores)  =>
            people.Where(_ => _.Sex == value.Sex);

        /// <inheritdoc />
        protected override string IdentifierName => "Sex";

        /// <inheritdoc />
        public override void SetValue(PersonEntity entity, Sex? newValue) => entity.Sex = newValue;

        /// <inheritdoc />
        public override Sex? ExistingValue(PersonEntity entity) => entity.Sex;
    }
}