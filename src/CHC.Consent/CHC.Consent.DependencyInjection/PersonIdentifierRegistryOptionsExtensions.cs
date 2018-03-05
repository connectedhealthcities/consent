using System;
using CHC.Consent.Common.Identity;

namespace CHC.Consent.DependencyInjection
{
    public static class PersonIdentifierRegistryOptionsExtensions
    {
        public static PersonIdentifierRegistryOptions Add<TIdentifier>(
            this PersonIdentifierRegistryOptions options,
            Action<PersonIdentifierOptionsBuilder<TIdentifier>> setup) 
            where TIdentifier : IPersonIdentifier
        {
            var identifierOptions = new PersonIdentifierOptions(typeof(TIdentifier));
            setup(new PersonIdentifierOptionsBuilder<TIdentifier>(identifierOptions));
            identifierOptions.Validate();
            
            options.IdentifierDescriptions.Add(identifierOptions);
            
            return options;
        }
        
    }
}