using System;
using System.Collections.Generic;
using System.Linq;

namespace CHC.Consent.Common
{
    public class Person
    {
        public long Id { get; protected set; }
        public string NhsNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Sex? Sex { get; set; }

        
        private readonly ISet<string> bradfordHosptialNumbers = new HashSet<string>();
        public IEnumerable<string> BradfordHosptialNumbers => bradfordHosptialNumbers.AsEnumerable();

        public Identifier[] GetIdentifier(IdentifierType type)
        {
            if (Equals(type, IdentifierType.NhsNumber))
                return new [] {IdentifierType.NhsNumber.Parse(NhsNumber)};
            if (Equals(type, IdentifierType.BradfordHospitalNumber))
                return BradfordHosptialNumbers.Select(IdentifierType.BradfordHospitalNumber.Parse).ToArray();
            throw new InvalidOperationException($"Don't know about identifiers of type '{type.ExternalId}'");
        }

        public bool AddHospitalNumber(string hospitalNumber) => bradfordHosptialNumbers.Add(hospitalNumber);
    }
}