using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CHC.Consent.Api.Infrastructure
{
    public class IIdentifierValueDtoJsonConverter : JsonConverter
    {
        private static readonly IDictionary<string, Type> types = IdentifierValueDtos.KnownDtoTypes
            .ToDictionary(_ => TypeExtensions.FriendlyId(_));
        public ILogger<IIdentifierValueDtoJsonConverter> Logger { get; }

        public IIdentifierValueDtoJsonConverter(ILogger<IIdentifierValueDtoJsonConverter> logger)
        {
            Logger = logger;
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var dto = ((IIdentifierValueDto)value);
            var innerValue = dto.Value;
            
            writer.WriteStartObject();
            writer.WritePropertyName("$type");
            writer.WriteValue(value.GetType().FriendlyId());
            writer.WritePropertyName("name");
            writer.WriteValue(dto.SystemName);
            writer.WritePropertyName("value");
            serializer.Serialize(writer, innerValue);
            writer.WriteEndObject();
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var typeName = (string) jObject["$type"];

            var type = types[typeName];
            var valueType = type.GetGenericArguments()[0];
            
            var name = (string) jObject["name"];
            var value = jObject["value"].ToObject(valueType, serializer);

            return Activator.CreateInstance(type, name, value);
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return typeof(IIdentifierValueDto).IsAssignableFrom(objectType);
        }
    }
}