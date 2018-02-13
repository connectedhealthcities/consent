using System;
using CHC.Consent.Common.Identity;
using Newtonsoft.Json.Serialization;

namespace CHC.Consent.Api.Infrastructure
{
    public class IdentifierRegistrySerializationBinder : ISerializationBinder
    {
        private readonly IdentifierRegistry registry;

        public IdentifierRegistrySerializationBinder(IdentifierRegistry registry)
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