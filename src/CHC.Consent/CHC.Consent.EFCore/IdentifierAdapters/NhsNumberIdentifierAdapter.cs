using System;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore.IdentifierAdapters
{
    public class NhsNumberIdentifierAdapter : SingleValueIdentifierAdapter<NhsNumberIdentifier, string>
    {
        /// <inheritdoc />
        public override IQueryable<TPerson> Filter<TPerson>(
            IQueryable<TPerson> people,
            NhsNumberIdentifier value,
            IStoreProvider stores) =>
            people.Where(_ => _.NhsNumber == value.Value);

        /// <inheritdoc />
        protected override string IdentifierName => "NHS Number";

        /// <inheritdoc />
        public override void SetValue(PersonEntity entity, string newValue) => entity.NhsNumber = newValue;

        /// <inheritdoc />
        public override string ExistingValue(PersonEntity entity) => entity.NhsNumber;
    }
}