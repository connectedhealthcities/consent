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
        public IIdentifierValueDto[] Identifiers { get; set; } = Array.Empty<IIdentifierValueDto>();        
    }
}