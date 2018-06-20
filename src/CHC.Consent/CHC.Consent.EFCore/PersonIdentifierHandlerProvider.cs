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
    public class PersonIdentifierHandlerProvider : IPersonIdentifierDisplayHandlerProvider
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

        public IEnumerable<IPersonIdentifierPersistanceHandler> AllPersistanceHandlers() =>
            PersonIdentifierRegistry.RegisteredTypes.Select(GetPersistanceHandler);

        public IPersonIdentifierPersistanceHandler GetPersistanceHandler(Type identifierType) =>
            HandlerProvider.GetPersistanceHandler(identifierType);

        public IPersonIdentifierPersistanceHandler GetPersistanceHandler(IPersonIdentifier identifier) =>
            GetPersistanceHandler(identifier.GetType());

        public IPersonIdentifierPersistanceHandler GetPersistanceHandler(string identifierName) =>
            GetPersistanceHandler(IdentifierType(identifierName));
        
        public IPersonIdentifierDisplayHandler GetDisplayHandler(Type identifierType) =>
            HandlerProvider.GetDisplayHandler(identifierType);

        public IPersonIdentifierDisplayHandler GetDisplayHandler(IPersonIdentifier identifier) =>
            GetDisplayHandler(identifier.GetType());

        public IPersonIdentifierDisplayHandler GetDisplayHandler(string identifierName) =>
            GetDisplayHandler(IdentifierType(identifierName));

        private Type IdentifierType(string identifierName)
        {
            if (PersonIdentifierRegistry.TryGetType(identifierName, out var identifierType))
            {
                return identifierType;
            }
            
            throw new InvalidOperationException($"Cannot find PersionIdentifier named '{identifierName}'");
        }
    }
}