using System;
using CHC.Consent.Api.Features.Identity.Dto;
using CHC.Consent.Common.Infrastructure.Definitions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CHC.Consent.Api.Infrastructure
{
    public class ConfigureJsonOptions : IConfigureOptions<MvcJsonOptions>
    {
        public readonly IServiceProvider services;

        /// <inheritdoc />
        public ConfigureJsonOptions(IServiceProvider services)
        {
            this.services = services;
        }

        /// <inheritdoc />
        public void Configure(MvcJsonOptions options)
        {
            ConfigureSerializer(options.SerializerSettings, services.GetService<ILogger<IIdentifierValueDtoJsonConverter>>());
        }

        public static JsonSerializerSettings ConfigureSerializer(
            JsonSerializerSettings settings,
            ILogger<IIdentifierValueDtoJsonConverter> logger = null)
        {
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.AddSwashbuckleNamesBinderFor<IDefinitionType>()
                .AddSwashbuckleNamesBinderFor<MatchSpecification>();
            settings.Converters.Add(
                new IIdentifierValueDtoJsonConverter(logger ?? NullLogger<IIdentifierValueDtoJsonConverter>.Instance));
            
            
            return settings;
        }
    }
}