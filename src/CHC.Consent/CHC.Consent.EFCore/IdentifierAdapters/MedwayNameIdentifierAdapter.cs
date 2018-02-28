using System.Collections.Generic;
using CHC.Consent.Common.Identity.Identifiers.Medway;
using CHC.Consent.Common.Infrastructure.Data;

namespace CHC.Consent.EFCore.IdentifierAdapters
{
    public class MedwayNameIdentifierAdapter : IdentifierAdapterBase<MedwayNameIdentifier>
    {
        public MedwayNameIdentifierAdapter() : base(new XmlIdentifierMarshaller<MedwayNameIdentifier>(valueType: "BIB4All.MedwayName"), MedwayNameIdentifier.TypeName)
        {
        }
    }
}