using CHC.Consent.Common.Identity.Identifiers;

namespace CHC.Consent.Common.Consent.Evidences
{
    public class EvidenceDefinition : DefinitionBase
    {
        public EvidenceDefinition(string name, IIdentifierType type) : base(name, type)
        {
            
        }

        public static EvidenceDefinition Create(string name, IIdentifierType type)
        {
            return new EvidenceDefinition(name, type);
        }
    }
}