using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore.IdentifierAdapters
{
    public class BradfordHospitalNumberIdentifierAdapter : 
        IIdentifierFilter<BradfordHospitalNumberIdentifier>,
        IIdentifierUpdater<BradfordHospitalNumberIdentifier>,
        IIdentifierRetriever<BradfordHospitalNumberIdentifier>
    {
        /// <inheritdoc />
        public IQueryable<PersonEntity> Filter(
            IQueryable<PersonEntity> people, BradfordHospitalNumberIdentifier value, IStoreProvider stores)
        {
            return
                people.Where(
                    _ => stores.Get<BradfordHospitalNumberEntity>().Any(
                        n => n.HospitalNumber == value.Value && n.Person.Id == _.Id));

        }

        /// <inheritdoc />
        public bool Update(PersonEntity person, BradfordHospitalNumberIdentifier value, IStoreProvider stores)
        {
            var hospitalNumbers = stores.Get<BradfordHospitalNumberEntity>();
            
            var existing = hospitalNumbers.SingleOrDefault(n => n.HospitalNumber == value.Value && n.Person.Id == person.Id);
            if (existing != null) return false;
            
            var personEntity = stores.Get<PersonEntity>().Get(person.Id);
            hospitalNumbers.Add(
                new BradfordHospitalNumberEntity {HospitalNumber = value.Value, Person = personEntity});
            return true;

        }

        /// <inheritdoc />
        public IEnumerable<BradfordHospitalNumberIdentifier> Get(PersonEntity person, IStoreProvider stores)
        {
            return stores.Get<BradfordHospitalNumberEntity>()
                .Where(n => n.Person.Id == person.Id)
                .Select(n => new BradfordHospitalNumberIdentifier(n.HospitalNumber))
                .ToArray();
        }
    }
}