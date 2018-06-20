using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure;

namespace CHC.Consent.EFCore
{
    /// <summary>
    /// Helper class to marry up <see cref="ITypeRegistry{TBaseType}"/> and <see cref="IdentifierHandlerProvider"/>
    /// </summary>
    public class PersonIdentifierHandlerProvider
    {
        private IIdentifierHandlerProvider HandlerProvider { get; }
        private ITypeRegistry<IPersonIdentifier> PersonIdentifierRegistry { get; }

        /// <inheritdoc />
        public PersonIdentifierHandlerProvider(
            IIdentifierHandlerProvider handlerProvider, 
            ITypeRegistry<IPersonIdentifier> personIdentifierRegistry)
        {
            HandlerProvider = handlerProvider;
            PersonIdentifierRegistry = personIdentifierRegistry;
        }

        public IPersonIdentifierPersistanceHandler GetPersistanceHandler(Type identifierType) =>
            HandlerProvider.GetPersistanceHandler(identifierType);

        public IPersonIdentifierPersistanceHandler GetPersistanceHandler(IPersonIdentifier identifier) =>
            GetPersistanceHandler(identifier.GetType());

        public IEnumerable<IPersonIdentifierPersistanceHandler> AllHandlers() =>
            PersonIdentifierRegistry.RegisteredTypes.Select(GetPersistanceHandler);
    }
}