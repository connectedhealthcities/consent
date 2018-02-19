using System;
using System.Collections.Generic;
using System.Linq;

namespace CHC.Consent.Common
{
    public class Person
    {
        public virtual long Id { get; set; }
        public virtual string NhsNumber { get; set; }
        public virtual DateTime? DateOfBirth { get; set; }
        public virtual Sex? Sex { get; set; }
        public virtual ushort? BirthOrder { get; set; }


        private readonly ISet<string> bradfordHosptialNumbers = new HashSet<string>();

        public virtual IEnumerable<string> BradfordHospitalNumbers => bradfordHosptialNumbers.AsEnumerable();

        public virtual bool AddHospitalNumber(string hospitalNumber) => bradfordHosptialNumbers.Add(hospitalNumber);
    }
}