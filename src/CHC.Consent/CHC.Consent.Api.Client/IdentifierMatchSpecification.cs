using System.Linq;

namespace CHC.Consent.Api.Client.Models
{
    public partial class IdentifierMatchSpecification
    {
        /// <inheritdoc />
        public IdentifierMatchSpecification(params IIdentifierValueDto[] identifierValueDtos) : this(identifierValueDtos.ToList())
        {
        }
    }
}