using System;
using System.Collections.Generic;
using System.Linq;

namespace CHC.Consent.Common
{
    public class Person
    {
        public long Id { get; set; }
        public string NhsNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Sex? Sex { get; set; }
        public ushort? BirthOrder { get; set; }


        private readonly ISet<string> bradfordHosptialNumbers = new HashSet<string>();

        public IEnumerable<string> BradfordHosptialNumbers => bradfordHosptialNumbers.AsEnumerable();

        public bool AddHospitalNumber(string hospitalNumber) => bradfordHosptialNumbers.Add(hospitalNumber);
    }
}