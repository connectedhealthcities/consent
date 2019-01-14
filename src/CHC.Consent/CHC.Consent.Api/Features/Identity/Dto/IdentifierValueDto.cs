using System;
using Newtonsoft.Json;

namespace CHC.Consent.Api.Features.Identity.Dto
{
    public interface IIdentifierValueDto
    {
        [JsonIgnore]
        string DefinitionSystemName { get; }
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
        string IIdentifierValueDto.DefinitionSystemName => Name;

        /// <inheritdoc />
        object IIdentifierValueDto.Value => Value;

        [JsonProperty]
        public string Name { get; set; }

        public T Value { get; set; }
    }
}