using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Entities;
using NeinLinq;

namespace CHC.Consent.EFCore.IdentifierAdapters
{
    public class DateOfBirthIdentifierAdapter : SingleValueIdentifierAdapter<DateOfBirthIdentifier, DateTime?>
    {
        /// <inheritdoc />
        public override IQueryable<PersonEntity> Filter(
            IQueryable<PersonEntity> people, DateOfBirthIdentifier value, IStoreProvider stores)

        {
            var dateOfBirth = value.DateOfBirth;
            return people.Where(_ => _.WasBornOn(dateOfBirth));
        }

        /// <inheritdoc />
        protected override string IdentifierName => "Date of Birth";

        /// <inheritdoc />
        public override void SetValue(PersonEntity entity, DateTime? newValue) => entity.DateOfBirth = newValue;

        /// <inheritdoc />
        public override DateTime? ExistingValue(PersonEntity entity) => entity.DateOfBirth;

        /// <inheritdoc />
        public override IEnumerable<DateOfBirthIdentifier> Get(PersonEntity person, IStoreProvider stores)
        {
            return person.DateOfBirth == null
                ? Enumerable.Empty<DateOfBirthIdentifier>()
                : new[] {new DateOfBirthIdentifier(person.DateOfBirth)};
        }
    }
    
    
    
    

    public static class DateOfBirthQueryableExtensions
    {
        [InjectLambda]
        public static bool WasBornOn(this PersonEntity person, DateTime? when)
        {
            return person.DateOfBirth == when;
        }

        public static Expression<Func<PersonEntity, DateTime?, bool>> WasBornOn() => (p, d) => p.DateOfBirth == d;
    }
}