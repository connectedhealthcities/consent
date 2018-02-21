using System;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore.IdentifierAdapters
{
    public class DateOfBirthIdentifierAdapter : IIdentifierFilter<DateOfBirthIdentifier>, IIdentifierUpdater<DateOfBirthIdentifier>
    {
        /// <inheritdoc />
        public IQueryable<TPerson> Filter<TPerson>(
            IQueryable<TPerson> people, DateOfBirthIdentifier value, IStoreProvider stores)
            where TPerson : Person => 
            people.Where(_ => _.DateOfBirth == value.DateOfBirth);

        /// <inheritdoc />
        public bool Update(Person person, DateOfBirthIdentifier value, IStoreProvider stores)
        {
            var entity = person as PersonEntity ?? stores.Get<PersonEntity>().Get(person.Id);
            if (entity == null)
            {
                throw new InvalidOperationException($"Cannot find person#{person.Id}");
            }

            if (entity.DateOfBirth == default)
            {
                entity.DateOfBirth = value.DateOfBirth;
            }
            else if (entity.DateOfBirth != value.DateOfBirth)
            {
                throw new InvalidOperationException($"Cannot change DateOfBirth for person#{person.Id}");
            }
            return true;
        }
    }
}