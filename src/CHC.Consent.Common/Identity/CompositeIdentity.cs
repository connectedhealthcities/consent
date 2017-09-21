using System.Xml.Linq;

namespace CHC.Consent.Common.Identity
{
    public class CompositeIdentity : Identity
    {    
        public virtual XDocument CompositeValue { get; set; }
    }
}