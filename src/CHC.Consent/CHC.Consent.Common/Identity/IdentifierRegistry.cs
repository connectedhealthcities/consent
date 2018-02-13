using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CHC.Consent.Common.Identity
{
    public class IdentifierRegistry : IEnumerable<(string name, Type type)>
    {
        private readonly IDictionary<string, Type> namesToTypes = new Dictionary<string, Type>();

        private readonly IDictionary<Type, (string Name, bool AllowMultiple)> typesToAttributes =
            new Dictionary<Type, (string Name, bool AllowMultiple)>();


        public void Add<T>() where T:IIdentifier
        {
            Add(typeof(T));
        }

        public void Add(Type identifierType)
        {
            var identifierAttributes = GetIdentifierAttributes(identifierType);
            namesToTypes.Add(identifierAttributes.Name, identifierType);
            typesToAttributes.Add(identifierType, identifierAttributes);
        }

        private static (string Name, bool AllowMultiple) GetIdentifierAttributes(Type identifierType)
        {
            var identifierAttribute = identifierType.GetCustomAttribute<IdentifierAttribute>();

            if (identifierAttribute != null) 
                return (identifierAttribute.Name, identifierAttribute.AllowMultipleValues);
            
            throw new ArgumentException($"Cannot get attributes for {identifierType} as it has no IdentifierAttribute");
        }

        public bool CanHaveMultipleValues<T>(T identifier) where T : IIdentifier
        {
            var identifierType = identifier.GetType();

            if (typesToAttributes.TryGetValue(identifierType, out var attributes)) return attributes.AllowMultiple;
            
            throw new InvalidOperationException($"Don't know about identifiers of type {identifierType}");
        }

        public Type this[string name] => namesToTypes[name];

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<(string name, Type type)> GetEnumerator()
        {
            return namesToTypes.Select(_ => (name:_.Key, type:_.Value)).GetEnumerator();
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