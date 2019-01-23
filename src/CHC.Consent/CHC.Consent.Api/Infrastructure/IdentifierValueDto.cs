using System;
using Newtonsoft.Json;

namespace CHC.Consent.Api.Infrastructure
{
    public interface IIdentifierValueDto
    {
        [JsonProperty("name")]
        string SystemName { get; set; }
        [JsonIgnore]
        object Value { get;  }
    }

    public static class IdentifierValueDtos
    {
        public static readonly Type[] KnownDtoTypes = {
            typeof(IdentifierValueDto<string>),
            typeof(IdentifierValueDto<DateTime>),
            typeof(IdentifierValueDto<long>),
            typeof(IdentifierValueDto<IIdentifierValueDto[]>)
        };
    }
    
    [JsonObject(ItemRequired = Required.Always)]
    public class IdentifierValueDto<T> : IIdentifierValueDto
    {
        /// <inheritdoc />
        public IdentifierValueDto(string name, T value)
        {
            Name = name;
            Value = value;
        }

        /// <inheritdoc />
        string IIdentifierValueDto.SystemName
        {
            get => Name;
            set => Name = value;
        }

        /// <inheritdoc />
        object IIdentifierValueDto.Value => Value;

        [JsonIgnore]
        public string Name { get; set; }

        public T Value { get; set; }
    }
}