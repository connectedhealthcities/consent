using System.Collections.Generic;

namespace CHC.Consent.Common.Identity
{
    public class Agency
    {
        public  AgencyIdentifier Id { get; set; }
        public string Name { get; set; }
        public string SystemName { get; set; }
        public ICollection<string> Fields { get; set; } = new List<string>();
    }
}