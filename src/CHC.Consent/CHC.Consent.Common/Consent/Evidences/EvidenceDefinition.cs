using System;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.Common.Infrastructure.Definitions;
using CHC.Consent.Common.Infrastructure.Definitions.Types;

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

        public Evidence Create(params Evidence[] values)
        {
            if (!(Type is CompositeDefinitionType))
            {
                throw new InvalidOperationException($"Cannot create Composite Evidence for '{SystemName}:{Type.SystemName}'");
            }

            return new Evidence(this, new CompositeIdentifierValue<Evidence>(values));
        }

        public Evidence Create(string value)
        {
            if (!(Type is StringDefinitionType))
            {
                throw new InvalidOperationException($"Cannot create Composite Evidence for '{SystemName}:{Type.SystemName}'");
            }

            return new Evidence(this, new SimpleIdentifierValue(value));
        }
        
    }
}