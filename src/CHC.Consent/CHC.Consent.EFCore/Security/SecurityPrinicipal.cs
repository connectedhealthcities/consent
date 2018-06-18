using System.Collections.Generic;

namespace CHC.Consent.EFCore.Security
{
    public abstract class SecurityPrinicipal
    {
        public long Id { get; protected set; }
        public ICollection<AccessControlEntity> Access { get; set; }
        
        public string Discriminator { get; set; }
    }
}