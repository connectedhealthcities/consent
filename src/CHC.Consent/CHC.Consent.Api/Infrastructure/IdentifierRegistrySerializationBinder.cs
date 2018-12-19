using System;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;
using Newtonsoft.Json.Serialization;

namespace CHC.Consent.Api.Infrastructure
{
    public class IdentifierRegistrySerializationBinder : ISerializationBinder
    {
        private readonly IdentifierDefinitionRegistry registry;

        public IdentifierRegistrySerializationBinder(IdentifierDefinitionRegistry registry)
        {
            this.registry = registry;
        }

        /// <inheritdoc />
        public Type BindToType(string assemblyName, string typeName)
        {
            return registry.ContainsKey(typeName) ? typeof(PersonIdentifier) : null;
        }

        /// <inheritdoc />
        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType == typeof(PersonIdentifier) ? "ignored" : null;
        }
    }
}