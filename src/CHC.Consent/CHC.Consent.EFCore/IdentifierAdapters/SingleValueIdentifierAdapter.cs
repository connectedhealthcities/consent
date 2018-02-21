using System;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore.IdentifierAdapters
{
    public abstract class SingleValueIdentifierAdapter<TIdentifier, TValue>
        : IIdentifierFilter<TIdentifier>
            , IIdentifierUpdater<TIdentifier>
        where TIdentifier : ISingleValueIdentifier<TValue>, IIdentifier
    {
        /// <inheritdoc />
        public abstract IQueryable<TPerson> Filter<TPerson>(
            IQueryable<TPerson> people,
            TIdentifier value,
            IStoreProvider stores) where TPerson : Person;

        /// <inheritdoc />
        public bool Update(Person person, TIdentifier value, IStoreProvider stores)
        {
            var entity = person as PersonEntity ?? stores.Get<PersonEntity>().Get(person.Id);
            if (entity == null)
            {
                throw new InvalidOperationException($"Cannot find person#{person.Id}");
            }

            if (CanUpdate(entity))
            {
                SetValue(entity, value);
            }
            else if (IsDifferent(entity, value))
            {
                throw new InvalidOperationException(
                    $"Cannot change {IdentifierName} to {value.Value} for person#{person.Id} {{ {IdentifierName} = {ExistingValue(entity)} }}");
            }
            return true;
        }

        protected abstract string IdentifierName { get;  }

        protected virtual bool IsDifferent(PersonEntity entity, TIdentifier value)
        {
            return !Equals(ExistingValue(entity), value.Value);
        }

        protected virtual void SetValue(PersonEntity entity, TIdentifier value)
        {
            SetValue(entity, value.Value);
        }

        public abstract void SetValue(PersonEntity entity, TValue newValue);

        protected virtual bool CanUpdate(PersonEntity entity) => Equals(ExistingValue(entity),default(TValue));

        public abstract TValue ExistingValue(PersonEntity entity);

    }
}