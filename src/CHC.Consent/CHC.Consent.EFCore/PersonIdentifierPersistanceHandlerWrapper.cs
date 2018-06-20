using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Identity;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore
{
    /// <summary>
    /// Wraps a typed <see cref="IPersonIdentifierPersistanceHandler{TIdentifier}"/> into an untyped <see cref="IPersonIdentifierPersistanceHandler"/> 
    /// </summary>
    /// <remarks>
    /// This bridges the gap between where we know which class of identifier we are dealing with, and where we don't 
    /// </remarks>
    public class PersonIdentifierPersistanceHandlerWrapper<TIdentifier> : IPersonIdentifierPersistanceHandler where TIdentifier : IPersonIdentifier
    {
        private readonly IPersonIdentifierPersistanceHandler<TIdentifier> persistanceHandler;

        /// <inheritdoc />
        public PersonIdentifierPersistanceHandlerWrapper(IPersonIdentifierPersistanceHandler<TIdentifier> persistanceHandler)
        {
            this.persistanceHandler = persistanceHandler;
        }

        /// <inheritdoc />
        public bool Update(PersonEntity person, IEnumerable<IPersonIdentifier> identifiers, IStoreProvider stores) =>
            persistanceHandler.Update(person, identifiers.Select(ConvertToCorrectType).ToArray(), stores);

        private static TIdentifier ConvertToCorrectType(IPersonIdentifier value)
        {
            switch (value)
            {
                    case TIdentifier typed: return typed;
                    default:
                        throw new InvalidOperationException(
                            $"Cannot convert {value.GetType()} to {typeof(TIdentifier)}");
            }
        }


        /// <inheritdoc />
        public IEnumerable<IPersonIdentifier> GetIdentifiers(PersonEntity person, IStoreProvider stores) 
            => persistanceHandler.Get(person, stores).Cast<IPersonIdentifier>();

        /// <inheritdoc />
        public IQueryable<PersonEntity> Filter(IQueryable<PersonEntity> people, IPersonIdentifier identifier, IStoreProvider storeProvider) 
            => persistanceHandler.Filter(people, ConvertToCorrectType(identifier), storeProvider);
    }
}