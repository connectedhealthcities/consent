using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Identity;
using Newtonsoft.Json;

namespace CHC.Consent.Api.Features.Identity.Dto
{
    public class MatchSpecification 
    {
        public IIdentifier[] Identifiers { get; set; } = Array.Empty<IIdentifier>();        
    }
}