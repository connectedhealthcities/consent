using System;

namespace CHC.Consent.Common.Identity.Identifiers
{
    public class NameIdentifier : IIdentifier
    {
        public string Given { get; set; }
        public string Family { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }
}