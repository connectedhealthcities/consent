using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CHC.Consent.Common.Infrastructure.Definitions;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CHC.Consent.Api.Infrastructure
{
    public class IdentifierTypeSerializationBinder : DefaultSerializationBinder
    {
        private readonly Lazy<IDictionary<string, Type>> typesByNameLazy;
        private readonly Lazy<IDictionary<Type, string>> namesByTypeLazy;
        
        private IDictionary<string, Type> TypesByName => typesByNameLazy.Value;
        private IDictionary<Type, string> NamesByType => namesByTypeLazy.Value;

        /// <inheritdoc />
        public IdentifierTypeSerializationBinder()
        {
            typesByNameLazy = new Lazy<IDictionary<string, Type>>(GetTypesByName);

            namesByTypeLazy =
                new Lazy<IDictionary<Type, string>>(() => TypesByName.ToDictionary(_ => _.Value, _ => _.Key));
        }

        private Dictionary<string, Type> GetTypesByName()
        {
            var entryAssembly = GetType().Assembly;
            return entryAssembly.GetReferencedAssemblies()
                .Select(Assembly.Load)
                .SelectMany(a => a.ExportedTypes.Where(t => typeof(IDefinitionType).IsAssignableFrom(t)))
                .Where(t => t != typeof(IDefinitionType))
                .ToDictionary(t => t.FriendlyId());
        }

        /// <inheritdoc />
        public override Type BindToType(string assemblyName, string typeName) =>
            TypesByName.TryGetValue(typeName, out var type)
                ? type
                : base.BindToType(assemblyName, typeName);

        /// <inheritdoc />
        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            if (!NamesByType.TryGetValue(serializedType, out typeName))
            {
                typeName = serializedType.FriendlyId();
            }
        }
    }
}