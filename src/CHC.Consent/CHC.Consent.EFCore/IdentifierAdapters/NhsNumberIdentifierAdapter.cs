using System;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore.IdentifierAdapters
{
    public class NhsNumberIdentifierAdapter : IIdentifierFilter<NhsNumberIdentifier>, IIdentifierUpdater<NhsNumberIdentifier>
    {
        /// <inheritdoc />
        public IQueryable<TPerson> Filter<TPerson>(
            IQueryable<TPerson> people, NhsNumberIdentifier value, IStoreProvider stores)
            where TPerson : Person => 
            people.Where(_ => _.NhsNumber == value.Value);

        /// <inheritdoc />
        public bool Update(Person person, NhsNumberIdentifier value, IStoreProvider stores)
        {
            
            var entity = person as PersonEntity ?? stores.Get<PersonEntity>().Get(person.Id);
            if (entity == null)
            {
                throw new InvalidOperationException($"Cannot find person#{person.Id}");
            }

            if (entity.NhsNumber == default)
            {
                entity.NhsNumber = value.Value;
            }
            else if (entity.NhsNumber != value.Value)
            {
                throw new InvalidOperationException($"Cannot change NhsNumber for person#{person.Id}");
            }
            return true;
        }
    }
}