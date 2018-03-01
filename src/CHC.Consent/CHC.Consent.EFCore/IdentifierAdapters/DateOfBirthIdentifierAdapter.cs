using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure.Data;

namespace CHC.Consent.EFCore.IdentifierAdapters
{
    public class DateOfBirthIdentifierAdapter : IdentifierAdapterBase<DateOfBirthIdentifier>
    {
        /// <inheritdoc />
        public DateOfBirthIdentifierAdapter() : base(new XmlIdentifierMarshaller<DateOfBirthIdentifier>("date"), DateOfBirthIdentifier.TypeName)
        {
        }
    }
}