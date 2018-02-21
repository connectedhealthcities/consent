using System;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore.IdentifierAdapters
{
    public class SexIdentifierAdapter : IIdentifierFilter<SexIdentifier>, IIdentifierUpdater<SexIdentifier>
    {
        /// <inheritdoc />
        public IQueryable<TPerson> Filter<TPerson>(
            IQueryable<TPerson> people,
            SexIdentifier value,
            IStoreProvider stores) where TPerson : Person =>
            people.Where(_ => _.Sex == value.Sex);

        /// <inheritdoc />
        public bool Update(Person person, SexIdentifier value, IStoreProvider stores)
        {
            var entity = person as PersonEntity ?? stores.Get<PersonEntity>().Get(person.Id);
            if (entity == null)
            {
                throw new InvalidOperationException($"Cannot find person#{person.Id}");
            }

            if (entity.Sex == default)
            {
                entity.Sex = value.Sex;
            }
            else if (entity.Sex != value.Sex)
            {
                throw new InvalidOperationException($"Cannot change Sex to {value.Sex} for person#{person.Id} {{Sex={person.Sex}}}");
            }
            return true;
        }
    }
}