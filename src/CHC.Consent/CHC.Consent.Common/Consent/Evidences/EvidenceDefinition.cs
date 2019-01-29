using CHC.Consent.Common.Infrastructure.Definitions;

namespace CHC.Consent.Common.Consent.Evidences
{
    public class EvidenceDefinition : DefinitionBase
    {
        public EvidenceDefinition(string name, IDefinitionType type) : base(name, type)
        {
            
        }

        public static EvidenceDefinition Create(string name, IDefinitionType type)
        {
            return new EvidenceDefinition(name, type);
        }
    }
}