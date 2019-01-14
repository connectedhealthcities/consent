using System;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;

namespace CHC.Consent.EFCore
{
    /// <summary>
    /// Helper class to marry up <see cref="ITypeRegistry{TBaseType}"/>
    /// </summary>
    public class PersonIdentifierHandlerProvider : IPersonIdentifierDisplayHandlerProvider
    {
        private IIdentifierHandlerProvider HandlerProvider { get; }
        private IdentifierDefinitionRegistry PersonIdentifierRegistry { get; }

        /// <inheritdoc />
        public PersonIdentifierHandlerProvider(
            IIdentifierHandlerProvider handlerProvider,
            IdentifierDefinitionRegistry personIdentifierRegistry)
        {
            HandlerProvider = handlerProvider;
            PersonIdentifierRegistry = personIdentifierRegistry;
        }

        private IPersonIdentifierDisplayHandler GetDisplayHandler(IdentifierDefinition identifierType) =>
            HandlerProvider.GetDisplayHandler(identifierType);

        public IPersonIdentifierDisplayHandler GetDisplayHandler(string identifierName) =>
            GetDisplayHandler(IdentifierType(identifierName));

        private IdentifierDefinition IdentifierType(string identifierName)
        {
            if (PersonIdentifierRegistry.TryGetValue(identifierName, out var identifierType))
            {
                return identifierType;
            }

            throw new InvalidOperationException($"Cannot find PersionIdentifier named '{identifierName}'");
        }
    }
}