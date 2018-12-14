using System;
using System.Collections.Generic;
using CHC.Consent.Common.Identity;
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
                Converters = { new StringEnumConverter(), new IdentityConverter() }
            };
        }
    }

    public class IdentityConverter : JsonConverter
    {
        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return typeof(IPersonIdentifier).IsAssignableFrom(objectType);
        }
    }
}