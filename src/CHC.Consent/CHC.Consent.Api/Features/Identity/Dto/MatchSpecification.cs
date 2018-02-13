using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Identity;

namespace CHC.Consent.Api.Features.Identity.Dto
{
    public class MatchSpecification : IEnumerable<IIdentifier>
    {
        public IIdentifier[] Identifiers { get; set; } = Array.Empty<IIdentifier>();

        /// <inheritdoc />
        public IEnumerator<IIdentifier> GetEnumerator() => Identifiers.Cast<IIdentifier>().GetEnumerator();
        
        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}