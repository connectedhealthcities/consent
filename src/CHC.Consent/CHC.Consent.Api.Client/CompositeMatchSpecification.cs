using System.Collections.Generic;

namespace CHC.Consent.Api.Client.Models
{
    public partial class CompositeMatchSpecification
    {
        /// <inheritdoc />
        public CompositeMatchSpecification(params MatchSpecification[] specifications) : 
            this((IList<MatchSpecification>)specifications)
        {
        }
    }
}