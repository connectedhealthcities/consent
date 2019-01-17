using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CHC.Consent.Api.Client.Models;
using CHC.Consent.Api.Features.Identity.Dto;
using CHC.Consent.Api.Infrastructure;
using IIdentifierValueDto = CHC.Consent.Api.Infrastructure.IIdentifierValueDto;

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

        public static Api.Client.Models.IdentifierValueDtoString Value(
            this Common.Identity.Identifiers.IdentifierDefinition definition,
            string value) =>
            new IdentifierValueDtoString(definition.SystemName, value);
        
        public static Api.Client.Models.IdentifierValueDtoDateTime Value(
            this Common.Identity.Identifiers.IdentifierDefinition definition,
            DateTime value) =>
            new IdentifierValueDtoDateTime(definition.SystemName, value);
        
        public static Api.Client.Models.IdentifierValueDtoInt64 Value(
            this Common.Identity.Identifiers.IdentifierDefinition definition,
            long value) =>
            new IdentifierValueDtoInt64(definition.SystemName, value);
        
        public static Api.Client.Models.IdentifierValueDtoIIdentifierValueDto Value(
            this Common.Identity.Identifiers.IdentifierDefinition definition,
            IEnumerable<Api.Client.Models.IIdentifierValueDto> value) =>
            new IdentifierValueDtoIIdentifierValueDto(definition.SystemName, value.ToArray()); 
        

        public static IIdentifierValueDto Dto<T>(
            this Common.Identity.Identifiers.IdentifierDefinition definition,
            T value)
            => new IdentifierValueDto<T>(definition.SystemName, value);
    }
}