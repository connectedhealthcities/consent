using CHC.Consent.Common.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace CHC.Consent.Api.Infrastructure
{
    public static class IdentifierRegistryJsonExtensions
    {
        public static JsonSerializerSettings CreateSerializerSettings(this ITypeRegistry identifierRegistry)
        {
            return new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = new IdentifierRegistrySerializationBinder(identifierRegistry),
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = { new StringEnumConverter() }
            };
        }
    }
}