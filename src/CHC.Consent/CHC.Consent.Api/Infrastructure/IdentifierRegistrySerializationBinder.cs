using System;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure;
using Newtonsoft.Json.Serialization;

namespace CHC.Consent.Api.Infrastructure
{
    public class IdentifierRegistrySerializationBinder : ISerializationBinder
    {
        private readonly ITypeRegistry registry;

        public IdentifierRegistrySerializationBinder(ITypeRegistry registry)
        {
            this.registry = registry;
        }

        /// <inheritdoc />
        public Type BindToType(string assemblyName, string typeName)
        {
            return registry[typeName];
        }

        /// <inheritdoc />
        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = registry.GetName(serializedType);
        }
    }
}