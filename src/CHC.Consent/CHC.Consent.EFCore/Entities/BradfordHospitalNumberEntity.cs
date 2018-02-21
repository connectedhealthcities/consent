using CHC.Consent.Common.Infrastructure.Data;

namespace CHC.Consent.EFCore.Entities
{
    internal class BradfordHospitalNumberEntity : IEntity
    {
        public long Id { get; set; }
        public string HospitalNumber { get; set; }
        public PersonEntity Person { get; set; }
    }
}