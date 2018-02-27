using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure;

namespace CHC.Consent.EFCore
{
    public class PersonIdentifierRegistry : TypeRegistry<IIdentifier, IdentifierAttribute>, IPersonIdentifierListChecker
    {
        private readonly Dictionary<Type, TypeAttributes> typesToAttributes =
            new Dictionary<Type, TypeAttributes>();

        readonly struct TypeAttributes
        {
            public string Name { get;  }
            public bool AllowMultiple { get; }
            public Type FilterType { get; }
            public Type UpdaterType { get; }
            public Type RetrieverType { get; }

            public TypeAttributes(
                string name, bool allowMultiple, Type filterType, Type updaterType, Type retrieverType)
            {
                Name = name;
                AllowMultiple = allowMultiple;
                FilterType = filterType;
                UpdaterType = updaterType;
                RetrieverType = retrieverType;
            }
        }

        public void Add<TIdentifier, TFilterUpdater>() 
            where TIdentifier : IIdentifier
            where TFilterUpdater: IIdentifierFilter<TIdentifier>, IIdentifierUpdater<TIdentifier>, IIdentifierRetriever<TIdentifier>
        {
            var identifierType = typeof(TIdentifier);
            var handlerType = typeof(TFilterUpdater);
            Add(identifierType, handlerType, handlerType, handlerType);
        }

        public void Add(Type identifierType, Type filterType, Type updaterType,Type retrieverType)
        {   
            CheckImplementsIdentifierHandler(filterType, typeof(IIdentifierFilter<>), identifierType);
            CheckImplementsIdentifierHandler(updaterType, typeof(IIdentifierUpdater<>), identifierType);
            CheckImplementsIdentifierHandler(retrieverType, typeof(IIdentifierRetriever<>), identifierType);

            var attribute = GetIdentifierAttribute(identifierType);
            base.Add(identifierType, attribute);
            typesToAttributes.Add(
                identifierType,
                new TypeAttributes(
                    attribute.Name,
                    attribute.AllowMultipleValues,
                    filterType,
                    updaterType,
                    retrieverType));
        }

        private static Type CheckImplementsIdentifierHandler(Type updaterType, Type handlerType, Type identifierType)
        {
            var updaterInterfaceType = handlerType.MakeGenericType(identifierType);
            if (!updaterInterfaceType.IsAssignableFrom(updaterInterfaceType))
                throw new ArgumentException($"{updaterType} is not {updaterInterfaceType}", nameof(updaterType));
            return updaterInterfaceType;
        }

        private TypeAttributes GetTypeAttributes(Type identifierType)
        {
            if (!typesToAttributes.TryGetValue(identifierType, out var attributes))
                throw new InvalidOperationException($"Don't know about identifiers of type {identifierType}");
            return attributes;
        }

        private bool CanHaveMultipleValues<T>(T identifier) where T : IIdentifier => 
            GetTypeAttributes(identifier.GetType()).AllowMultiple;

        public Type GetFilterType(Type identifierType) => GetTypeAttributes(identifierType).FilterType;

        public Type GetRetrieverType(Type identifierType) => GetTypeAttributes(identifierType).RetrieverType;
        public Type GetUpdaterType(Type identifierType) => GetTypeAttributes(identifierType).UpdaterType;


        public void EnsureHasNoInvalidDuplicates(IEnumerable<IIdentifier> identifiers)
        {
            var invalidDuplicates =
                identifiers
                    .Where(identifer => !CanHaveMultipleValues<IIdentifier>(identifer))
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