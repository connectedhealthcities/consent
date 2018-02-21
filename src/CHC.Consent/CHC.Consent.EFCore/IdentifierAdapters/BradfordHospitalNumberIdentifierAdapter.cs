using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore.IdentifierAdapters
{
    public class BradfordHospitalNumberIdentifierAdapter : IIdentifierFilter<BradfordHospitalNumberIdentifier>, IIdentifierUpdater<BradfordHospitalNumberIdentifier>
    {
        /// <inheritdoc />
        public IQueryable<TPerson> Filter<TPerson>(
            IQueryable<TPerson> people, BradfordHospitalNumberIdentifier value, IStoreProvider stores) where TPerson:Person
        {
            return
                people.Where(
                    _ => stores.Get<BradfordHospitalNumberEntity>().Any(
                        n => n.HospitalNumber == value.Value && n.Person.Id == _.Id));

        }

        /// <inheritdoc />
        public bool Update(Person person, BradfordHospitalNumberIdentifier value, IStoreProvider stores)
        {
            var hospitalNumbers = stores.Get<BradfordHospitalNumberEntity>();
            
            var existing = hospitalNumbers.SingleOrDefault(n => n.HospitalNumber == value.Value && n.Person.Id == person.Id);
            if (existing != null) return false;
            
            var personEntity = stores.Get<PersonEntity>().Get(person.Id);
            hospitalNumbers.Add(
                new BradfordHospitalNumberEntity {HospitalNumber = value.Value, Person = personEntity});
            return true;

        }
    }
}