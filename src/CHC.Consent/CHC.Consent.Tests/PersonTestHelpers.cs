using CHC.Consent.Common;

namespace CHC.Consent.Tests
{
    public static class PersonTestHelpers
    {
        public static Person WithBradfordHosptialNumbers(this Person person, params string[] hospitalNumbers)
        {
            foreach (var hospitalNumber in hospitalNumbers)
            {
                person.AddHospitalNumber(hospitalNumber);
            }

            return person;
        }
    }
}