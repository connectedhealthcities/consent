using System.Collections.Generic;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure;

namespace CHC.Consent.DependencyInjection
{
    public class PersonIdentifierRegistryOptions
    {
        public List<PersonIdentifierOptions> IdentifierDescriptions { get; } = new List<PersonIdentifierOptions>();

        public TypeRegistry<IPersonIdentifier, IdentifierAttribute> CreateTypeRegistry()
        {
            var registry = new TypeRegistry<IPersonIdentifier, IdentifierAttribute>();
            foreach (var description in IdentifierDescriptions)
            {
                registry.Add(description.IdentifierType, description.TypeName);
            }

            return registry;
        }

        public PersonIdentifierListChecker CreateListChecker()
        {
            var checker = new PersonIdentifierListChecker();
            foreach (var description in IdentifierDescriptions)
            {
                checker.Add(description.IdentifierType, description.CanHaveDuplicates);
            }

            return checker;
        }
    }
}