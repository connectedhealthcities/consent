using System.Collections.Generic;

namespace CHC.Consent.DependencyInjection
{
    public class PersonIdentifierRegistryOptions
    {
        public List<PersonIdentifierOptions> IdentifierDescriptions { get; } = new List<PersonIdentifierOptions>();
    }
}