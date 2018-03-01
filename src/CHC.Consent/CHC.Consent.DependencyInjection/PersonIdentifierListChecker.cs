using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Identity;

namespace CHC.Consent.DependencyInjection
{
    public class PersonIdentifierListChecker : IPersonIdentifierListChecker
    {
        private readonly Dictionary<Type, bool> identifierMeta = new Dictionary<Type, bool>();

        public void Add(Type identiferType, bool canHaveDuplicates)
        {
            if(!typeof(IIdentifier).IsAssignableFrom(identiferType)) 
                throw new ArgumentException("Must be an IIdentifier", nameof(identiferType));

            identifierMeta.Add(identiferType, canHaveDuplicates);
        }
        
        /// <inheritdoc />
        public void EnsureHasNoInvalidDuplicates(IEnumerable<IIdentifier> identifiers)
        {
            foreach (var identifierGroup in identifiers.GroupBy(_ => _.GetType()))
            {
                if (!identifierMeta.TryGetValue(identifierGroup.Key, out var canHaveDuplicates))
                {
                    throw new InvalidOperationException($"{identifierGroup.Key} is not a know Identifier Type");
                }

                if (!canHaveDuplicates && identifierGroup.Count() > 1)
                {
                    throw new InvalidOperationException($"More than one {identifierGroup.Key} sent");   
                }
            }
        }
    }
}