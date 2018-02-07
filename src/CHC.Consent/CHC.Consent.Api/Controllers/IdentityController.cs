using System;
using System.Collections.Generic;
using CHC.Consent.Common;
using Microsoft.AspNetCore.Mvc;

namespace CHC.Consent.Api.Controllers
{
    [Route("/identities")]
    public class IdentityController : Controller
    {
        [HttpPost]
        public IActionResult PostPerson(PersonSpecification person)
        {
            var identityValues = new List<Identifier>();
            var identifiersById = new Dictionary<string, Identifier>();
            foreach (var specification in person.Identifiers)
            {
                var type = GetIdentityTypeFor(specification);
                var value = type.Parse(specification.Value);

                if (string.IsNullOrEmpty(specification.Id)) continue;
                
                if (identifiersById.ContainsKey(specification.Id))
                {
                    throw new InvalidOperationException($"Already have an identifier for id#{specification.Id}");
                }

                identifiersById[specification.Id] = value;
            }

            foreach (var spec in person.MatchBy)
            {
                
            }
        }

        private IdentifierType GetIdentityTypeFor(IdentifierSpecification identifier)
        {
            throw new NotImplementedException("Get identity type from somewhere");
            
        }
    }

    public class PersonSpecification
    {
        public IdentifierSpecification[] Identifiers { get; set; } = Array.Empty<IdentifierSpecification>();
        
        public MatchSpecification[] MatchBy { get; set; } = Array.Empty<MatchSpecification>()
    }

    public class MatchSpecification
    {
        public MatchIdentifierSpecification[] Identifiers { get; set; } = Array.Empty<MatchIdentifierSpecification>();
        
    }
    
    public class MatchIdentifierSpecification
    {
        public MatchBy MatchBy { get; set; }
        public string IdOrType { get; set; }
    }

    public enum MatchBy
    {
        Id,
        Type
    }

    public class IdentifierSpecification
    {
        public string Type { get; set; }
        public string Id { get; set; }
        public string Value { get; set; }
    }
}