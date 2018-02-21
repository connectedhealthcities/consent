using System;
using System.Linq;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore.IdentifierAdapters
{
    public class DateOfBirthIdentifierAdapter : SingleValueIdentifierAdapter<DateOfBirthIdentifier, DateTime?>
    {
        /// <inheritdoc />
        public override IQueryable<TPerson> Filter<TPerson>(
            IQueryable<TPerson> people, DateOfBirthIdentifier value, IStoreProvider stores) => 
            people.Where(_ => _.DateOfBirth == value.DateOfBirth);

        /// <inheritdoc />
        protected override string IdentifierName => "Date of Birth";

        /// <inheritdoc />
        public override void SetValue(PersonEntity entity, DateTime? newValue) => entity.DateOfBirth = newValue;

        /// <inheritdoc />
        public override DateTime? ExistingValue(PersonEntity entity) => entity.DateOfBirth;
    }
}