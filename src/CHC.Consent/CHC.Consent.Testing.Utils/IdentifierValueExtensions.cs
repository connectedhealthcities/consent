using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Api.Client.Models;
using CHC.Consent.Api.Infrastructure;
using IdentifierDefinition = CHC.Consent.Common.Identity.Identifiers.IdentifierDefinition;
using IIdentifierValueDto = CHC.Consent.Api.Client.Models.IIdentifierValueDto;

namespace CHC.Consent.Testing.Utils
{
    public static class IdentifierValueExtensions
    {
        /*public static Api.Client.Models.IIdentifierValueDto Value<T>(
            this Common.Identity.Identifiers.IdentifierDefinition definition,
            T value)
        {
            switch (value)
            {
                case string @string: return new IdentifierValueDtoString(definition.SystemName, @string);
                case DateTime @date: return new IdentifierValueDtoDateTime(definition.SystemName, @date.Date);
                case long @long: return new IdentifierValueDtoInt64(definition.SystemName, @long);
                case IEnumerable<Api.Client.Models.IIdentifierValueDto> values: return new IdentifierValueDtoIIdentifierValueDto(definition.SystemName, values.ToList());
                default: throw new ArgumentOutOfRangeException(nameof(value), $"Cannot handle '{value?.GetType()}'");
            }
        }*/

        public static IdentifierValueDtoString Value(
            this IdentifierDefinition definition,
            string value) =>
            new IdentifierValueDtoString(definition.SystemName, value);
        
        public static IdentifierValueDtoDateTime Value(
            this IdentifierDefinition definition,
            DateTime value) =>
            new IdentifierValueDtoDateTime(definition.SystemName, value);
        
        public static IdentifierValueDtoInt64 Value(
            this IdentifierDefinition definition,
            long value) =>
            new IdentifierValueDtoInt64(definition.SystemName, value);
        
        public static IdentifierValueDtoIIdentifierValueDto Value(
            this IdentifierDefinition definition,
            IEnumerable<IIdentifierValueDto> value) =>
            new IdentifierValueDtoIIdentifierValueDto(definition.SystemName, value.ToArray()); 
        

        public static Api.Infrastructure.IIdentifierValueDto Dto<T>(
            this IdentifierDefinition definition,
            T value)
            => new IdentifierValueDto<T>(definition.SystemName, value);
    }
}