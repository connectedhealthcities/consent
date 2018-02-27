using CHC.Consent.Common.Infrastructure;

namespace CHC.Consent.Common.Consent
{
    public class EvidenceRegistry : TypeRegistry<Evidence, EvidenceAttribute>
    {
        public void Add<T>() where T : Evidence => base.Add(typeof(T), GetIdentifierAttribute(typeof(T)));
    }
}