namespace CHC.Consent.EFCore
{
    internal class BradfordHospitalNumberEntity
    {
        public long Id { get; set; }
        public string HospitalNumber { get; set; }
        public PersonEntity PersonEntity { get; set; }
    }
}