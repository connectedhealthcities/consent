using System.Collections.Generic;

namespace CHC.Consent.EFCore.Security
{
    public class AccessControlList
    {
        public long Id { get; protected set; }
        public ICollection<AccessControlEntity> Entries { get; set; } = new List<AccessControlEntity>();
        public string Description { get; set; }
    }
}