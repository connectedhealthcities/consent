using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Api.Client.Models;

namespace CHC.Consent.Testing.Utils
{
    public static class ClientIdentifierValues
    {
        public static CHC.Consent.Api.Client.Models.IdentifierValueDtoIIdentifierValueDto Address(
            string line1 = null,
            string line2 = null,
            string line3 = null,
            string line4 = null,
            string line5 = null,
            string postcode = null)
        {
            return Identifiers.Definitions.Address.Value(AddressLines(line1,line2,line3, line4,line5, postcode).ToArray());
        }

        private static IEnumerable<IIdentifierValueDto> AddressLines(
            string line1 = null,
            string line2 = null,
            string line3 = null,
            string line4 = null,
            string line5 = null,
            string postcode = null
        )
        {
            if(line1 != null) yield return Identifiers.Definitions.AddressLine1.Value(line1);
            if(line2 != null) yield return Identifiers.Definitions.AddressLine2.Value(line2);
            if(line2 != null) yield return Identifiers.Definitions.AddressLine3.Value(line2);
            if(line4 != null) yield return Identifiers.Definitions.AddressLine4.Value(line4);
            if(line5 != null) yield return Identifiers.Definitions.AddressLine5.Value(line5);
            if(postcode != null) yield return Identifiers.Definitions.AddressPostcode.Value(postcode);
        }

        

        public static CHC.Consent.Api.Client.Models.IdentifierValueDtoIIdentifierValueDto Name(
            string firstName=null,
            string lastName=null
        )
        {
            var parts = new List<IIdentifierValueDto>();
            if(firstName != null) parts.Add(Identifiers.Definitions.FirstName.Value(firstName));
            if(lastName != null) parts.Add(Identifiers.Definitions.LastName.Value(lastName));
            return Identifiers.Definitions.Name.Value(parts.ToArray());
        }
    }
}