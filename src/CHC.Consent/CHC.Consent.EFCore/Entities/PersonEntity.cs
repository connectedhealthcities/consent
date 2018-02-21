using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CHC.Consent.Common;

[assembly:InternalsVisibleTo("CHC.Consent.EFCore.Tests")]

namespace CHC.Consent.EFCore.Entities
{
    public class PersonEntity : Person
    {
        public override ushort? BirthOrder
        {
            get => (ushort?) BirthOrderValue;
            set => BirthOrderValue = value;
        }

        internal int? BirthOrderValue { get; set; }
        
        internal virtual ICollection<BradfordHospitalNumberEntity> BradfordHospitalNumberEntities { get; set; } = new List<BradfordHospitalNumberEntity>();

        /// <inheritdoc />
        public override IEnumerable<string> BradfordHospitalNumbers => BradfordHospitalNumberEntities.Select(_ => _.HospitalNumber);

        /// <inheritdoc />
        public override bool AddHospitalNumber(string hospitalNumber)
        {
            if (BradfordHospitalNumberEntities.Any(_ => _.HospitalNumber == hospitalNumber))
                return false;
            else
            {
                BradfordHospitalNumberEntities.Add(new BradfordHospitalNumberEntity{ HospitalNumber = hospitalNumber, Person = this });
                return true;
            }
        }
    }
}