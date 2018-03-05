using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore
{
    /// <summary>
    /// Wraps a typed <see cref="IPersonIdentifierHandler{T}"/> into an untyped <see cref="IPersonIdentifierHandler"/> 
    /// </summary>
    /// <remarks>
    /// This bridges the gap between where we know which class of identifier we are dealing with, and where we don't 
    /// </remarks>
    public class PersonIdentifierHandlerWrapper<TIdentifier> : IPersonIdentifierHandler where TIdentifier : IPersonIdentifier
    {
        private readonly IPersonIdentifierHandler<TIdentifier> handler;

        /// <inheritdoc />
        public PersonIdentifierHandlerWrapper(IPersonIdentifierHandler<TIdentifier> handler)
        {
            this.handler = handler;
        }

        /// <inheritdoc />
        public bool Update(PersonEntity person, IPersonIdentifier value, IStoreProvider stores) =>
            handler.Update(person, GetTypedIdentifier(value), stores);

        private static TIdentifier GetTypedIdentifier(IPersonIdentifier value)
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
        public IEnumerable<IPersonIdentifier> Get(PersonEntity person, IStoreProvider stores) 
            => handler.Get(person, stores).Cast<IPersonIdentifier>();

        /// <inheritdoc />
        public IQueryable<PersonEntity> Filter(IQueryable<PersonEntity> people, IPersonIdentifier identifier, IStoreProvider storeProvider) 
            => handler.Filter(people, GetTypedIdentifier(identifier), storeProvider);
    }
}