using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CHC.Consent.Common.Infrastructure;

namespace CHC.Consent.Common.Identity
{
    public class PersonIdentifierRegistry : TypeRegistry<IIdentifier, IdentifierAttribute>
    {
        private readonly IDictionary<string, Type> namesToTypes = new Dictionary<string, Type>();

        private readonly IDictionary<Type, (string Name, bool AllowMultiple)> typesToAttributes =
            new Dictionary<Type, (string Name, bool AllowMultiple)>();


        /// <inheritdoc />
        protected override void Add(Type identifierType, IdentifierAttribute attribute)
        {
            base.Add(identifierType, attribute);
            typesToAttributes.Add(identifierType, (attribute.Name, attribute.AllowMultipleValues));
        }

        private bool CanHaveMultipleValues<T>(T identifier) where T : IIdentifier
        {
            var identifierType = identifier.GetType();

            if (typesToAttributes.TryGetValue(identifierType, out var attributes)) return attributes.AllowMultiple;
            
            throw new InvalidOperationException($"Don't know about identifiers of type {identifierType}");
        }

        public void EnsureHasNoInvalidDuplicates(IEnumerable<IIdentifier> identifiers)
        {
            var invalidDuplicates =
                identifiers
                    .Where(identifer => !CanHaveMultipleValues(identifer))
                    .GroupBy(identifier => identifier.GetType())
                    .Where(identifiersByType => identifiersByType.Count() > 1)
                    .ToArray();

            if (!invalidDuplicates.Any()) return;
            
            var errors = new StringBuilder();
            errors.AppendLine("Invalid Duplicate identifiers found:");
            foreach (var duplicate in invalidDuplicates)
            {
                errors.AppendFormat("\t{0} has values: ", duplicate.Key);
                foreach (var value in duplicate)
                {
                    errors.AppendFormat("{0}, ", value);
                }
                    
                errors.AppendLine();
            }
            
            throw new InvalidOperationException(errors.ToString());
        }
    }
}